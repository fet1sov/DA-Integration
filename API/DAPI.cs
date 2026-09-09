using DA_Integration.Handlers;
using DA_Integration.Utils;
using NuGet.Protocol;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DA_Integration.API
{
    public class DAPI : IDisposable
    {
        private const string TokenEndpoint = "https://www.donationalerts.com/oauth/token";
        private const string UserInfoEndpoint = "https://www.donationalerts.com/api/v1/user/oauth";
        private const string CentrifugeSubscribeEndpoint = "https://www.donationalerts.com/api/v1/centrifuge/subscribe";
        private const string WsEndpoint = "wss://centrifugo.donationalerts.com/connection/websocket";
        private const string Scopes = "oauth-user-show oauth-donation-subscribe oauth-donation-index oauth-custom_alert-store";

        private static readonly HttpClient HttpClient = new HttpClient();
        private HttpListener? _httpListener = null;
        private ClientWebSocket? _webSocket = null;
        private CancellationTokenSource? _wsCancellationTokenSource = null;

        private string _socketClientId = String.Empty;
        private string _channelId = String.Empty;
        private string _channelConnectionToken = String.Empty;

        #region DTO Models

        public class AccessResponse
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
        }

        public class UserData
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("socket_connection_token")]
            public string SocketConnectionToken { get; set; }
        }

        public class UserDataResponse
        {
            [JsonPropertyName("data")]
            public UserData Data { get; set; }
        }

        public class Channel
        {
            [JsonPropertyName("channel")]
            public string ChannelName { get; set; }

            [JsonPropertyName("token")]
            public string Token { get; set; }
        }

        public class SubscribeDataResponse
        {
            [JsonPropertyName("channels")]
            public List<Channel> Channels { get; set; }
        }

        public class WebSocketConnectResponse
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("result")]
            public ConnectResult Result { get; set; }
        }

        public class ConnectResult
        {
            [JsonPropertyName("client")]
            public string Client { get; set; }
        }

        public class DonationResponse
        {
            [JsonPropertyName("result")]
            public DonationResult Result { get; set; }
        }

        public class DonationResult
        {
            [JsonPropertyName("channel")]
            public string Channel { get; set; }

            [JsonPropertyName("data")]
            public DonationData Data { get; set; }
        }

        public class DonationData
        {
            [JsonPropertyName("data")]
            public DonationUserData Data { get; set; }
        }

        public class DonationUserData
        {
            [JsonPropertyName("username")]
            public string Username { get; set; }

            [JsonPropertyName("amount")]
            public int Amount { get; set; }

            [JsonPropertyName("currency")]
            public string Currency { get; set; }
        }

        private class CentrifugeAuthPayload
        {
            [JsonPropertyName("params")]
            public CentrifugeAuthParams Params { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }
        }

        private class CentrifugeAuthParams
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }

        private class CentrifugeSubscribePayload
        {
            [JsonPropertyName("params")]
            public CentrifugeSubscribeParams Params { get; set; }

            [JsonPropertyName("method")]
            public int Method { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }
        }

        private class CentrifugeSubscribeParams
        {
            [JsonPropertyName("channel")]
            public string Channel { get; set; }

            [JsonPropertyName("token")]
            public string Token { get; set; }
        }

        #endregion

        public async Task InitializeAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(ConfigManager.Config.RefreshToken))
                {
                    bool refreshed = await RefreshAccessTokenAsync();
                    if (refreshed)
                    {
                        await StartWebSocketFlowAsync();
                        return;
                    }
                }

                StartLocalOAuthServer();
            }
            catch (Exception ex)
            {
                Debugger.ErrorOutput($"Ошибка при инициализации DAPI: {ex.Message}");
            }
        }

        #region Embedded REST / OAuth Listener

        private void StartLocalOAuthServer()
        {
            string authUrl = $"https://www.donationalerts.com/oauth/authorize" +
                             $"?client_id={ConfigManager.Config.ClientId}" +
                             $"&redirect_uri={ConfigManager.Config.RedirectUrl}" +
                             $"&response_type=code" +
                             $"&scope=oauth-user-show%20oauth-donation-subscribe%20oauth-donation-index%20oauth-custom_alert-store";

            Debugger.ErrorOutput("Требуется авторизация в DonationAlerts!");
            Debugger.MessageOutput($"Для авторизации перейдите по этой ссылке: {authUrl}");

            try
            {
                _httpListener?.Stop();
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://localhost:80/");
                _httpListener.Prefixes.Add("http://127.0.0.1:80/");
                _httpListener.Start();

                Task.Run(ListenForOAuthCallbackAsync);
            }
            catch (Exception ex)
            {
                Debugger.ErrorOutput($"Не удалось запустить локальный HTTP-сервер: {ex.Message}");
            }
        }

        private async Task ListenForOAuthCallbackAsync()
        {
            while (_httpListener != null && _httpListener.IsListening)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    var request = context.Request;
                    var response = context.Response;

                    string code = request.QueryString["code"];

                    string responseString;
                    if (!string.IsNullOrEmpty(code))
                    {
                        responseString = "<html><head><meta charset='utf-8'></head><body><h2>Авторизация DonationAlerts прошла успешно!</h2><p>Можете закрыть эту вкладку и вернуться в игру.</p></body></html>";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        response.OutputStream.Close();

                        Debugger.SuccessOutput("Код авторизации успешно перехвачен!");

                        _httpListener.Stop();

                        await GetAccessTokenByCodeAsync(code);
                        await StartWebSocketFlowAsync();
                        break;
                    }
                    else
                    {
                        responseString = "<html><head><meta charset='utf-8'></head><body><h2>Ошибка: код авторизации не получен.</h2></body></html>";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debugger.ErrorOutput($"Ошибка обработки OAuth-запроса: {ex.Message}");
                }
            }
        }

        #endregion

        #region Token Management

        private async Task GetAccessTokenByCodeAsync(string code)
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", ConfigManager.Config.ClientId },
                { "client_secret", ConfigManager.Config.ClientSecret },
                { "redirect_uri", ConfigManager.Config.RedirectUrl },
                { "code", code }
            };

            await RequestTokenAsync(values);
        }

        public async Task<bool> RefreshAccessTokenAsync()
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", ConfigManager.Config.RefreshToken },
                { "client_id", ConfigManager.Config.ClientId },
                { "client_secret", ConfigManager.Config.ClientSecret },
                { "scope", Scopes }
            };

            return await RequestTokenAsync(values);
        }

        private async Task<bool> RequestTokenAsync(Dictionary<string, string> bodyValues)
        {
            var content = new FormUrlEncodedContent(bodyValues);

            var response = await HttpClient.PostAsync(TokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                Debugger.ErrorOutput($"Ошибка запроса токена! Статус: {response.StatusCode}");
                Debugger.ErrorOutput(string.Join(", ", bodyValues.Select(kvp => $"[{kvp.Key}: {kvp.Value}]")));
                Debugger.ErrorOutput($"Ошибка запроса токена! Контент: {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            string json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<AccessResponse>(json);

            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                Debugger.ErrorOutput("Не удалось распарсить ответ с токеном.");
                return false;
            }

            ConfigManager.Config.AccessToken = tokenData.AccessToken;
            ConfigManager.Config.RefreshToken = tokenData.RefreshToken;
            ConfigManager.Save();

            Debugger.SuccessOutput("Токены успешно получены и сохранены!");
            return true;
        }
        #endregion

        #region WebSocket & Centrifugo

        private async Task StartWebSocketFlowAsync()
        {
            if (string.IsNullOrEmpty(ConfigManager.Config.AccessToken))
            {
                Debugger.ErrorOutput("Отсутствует access_token!");
                return;
            }

            string socketToken = await GetSocketTokenAsync();
            if (!string.IsNullOrEmpty(socketToken))
            {
                _ = Task.Run(() => InitWebSocketAsync(socketToken));
            }
        }

        private async Task<string> GetSocketTokenAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ConfigManager.Config.AccessToken);
                var response = await HttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Debugger.ErrorOutput("Не удалось получить Socket Token. Попытка обновить Access Token...");
                    bool refreshed = await RefreshAccessTokenAsync();
                    if (refreshed)
                    {
                        return await GetSocketTokenAsync();
                    }

                    StartLocalOAuthServer();
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<UserDataResponse>(json);

                _channelId = userResponse.Data.Id.ToString();
                return userResponse.Data.SocketConnectionToken;
            }
        }

        private async Task InitWebSocketAsync(string socketToken)
        {
            try
            {
                _wsCancellationTokenSource?.Cancel();
                _wsCancellationTokenSource = new CancellationTokenSource();

                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();

                await _webSocket.ConnectAsync(new Uri(WsEndpoint), _wsCancellationTokenSource.Token);
                Debugger.SuccessOutput("WebSocket подключен. Авторизация Centrifugo...");

                var authPayload = new CentrifugeAuthPayload
                {
                    Params = new CentrifugeAuthParams { Token = socketToken },
                    Id = 1
                };
                await SendTextAsync(JsonSerializer.Serialize(authPayload));

                var buffer = new byte[1024 * 8];
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _wsCancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnSocketMessage(message);
                }
            }
            catch (Exception ex)
            {
                Debugger.ErrorOutput($"Ошибка WebSocket потока: {ex.Message}");
            }
        }

        private async Task SendTextAsync(string text)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;
            var bytes = Encoding.UTF8.GetBytes(text);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _wsCancellationTokenSource?.Token ?? CancellationToken.None);
        }

        private async void OnSocketMessage(string message)
        {
            try
            {
                if (ConfigManager.Config.Debug)
                {
                    Debugger.MessageOutput($"Сообщение сокета: {message}");
                }

                if (message.StartsWith("{\"id\":1"))
                {
                    var authResponse = JsonSerializer.Deserialize<WebSocketConnectResponse>(message);
                    if (authResponse?.Result != null)
                    {
                        _socketClientId = authResponse.Result.Client;
                        await SubscribeChannelAsync();
                    }
                }
                else if (message.Contains("\"channel\":\"$alerts:donation"))
                {
                    var donation = JsonSerializer.Deserialize<DonationResponse>(message);
                    var userData = donation?.Result?.Data?.Data;
                    if (userData != null)
                    {
                        DonateHandler.HandleDonate(userData.Username, userData.Currency, userData.Amount);
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.ErrorOutput($"Ошибка обработки сообщения WebSocket: {ex.Message}");
            }
        }

        private async Task SubscribeChannelAsync()
        {
            var requestBody = new
            {
                channels = new[] { $"$alerts:donation_{_channelId}" },
                client = _socketClientId
            };

            using (var request = new HttpRequestMessage(HttpMethod.Post, CentrifugeSubscribeEndpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ConfigManager.Config.AccessToken);
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Debugger.ErrorOutput("Не удалось получить токен подключения к каналу.");
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();
                var subData = JsonSerializer.Deserialize<SubscribeDataResponse>(json);

                if (subData?.Channels != null && subData.Channels.Count > 0)
                {
                    _channelConnectionToken = subData.Channels[0].Token;

                    var subscribePayload = new CentrifugeSubscribePayload
                    {
                        Params = new CentrifugeSubscribeParams
                        {
                            Channel = $"$alerts:donation_{_channelId}",
                            Token = _channelConnectionToken
                        },
                        Method = 1,
                        Id = 2
                    };

                    await SendTextAsync(JsonSerializer.Serialize(subscribePayload));
                    Debugger.SuccessOutput("Успешная подписка на канал донатов!");
                }
            }
        }

        #endregion

        public void Dispose()
        {
            try
            {
                if (_httpListener != null && _httpListener.IsListening)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }

                _wsCancellationTokenSource?.Cancel();
                _webSocket?.Dispose();
            }
            catch
            {
            }
        }
    }
}