using System;
using WebSocketSharp;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

/*
    DonationAlerts API in C#
    Based on work with Centrifugo server

    ╔═══╗╔═╗╔═╗╔═══╗╔╗─╔╗╔═══╗╔╗─╔╗╔═══╗╔════╗
    ╚╗╔╗║╚╗╚╝╔╝║╔═╗║║║─║║║╔═╗║║║─║║║╔═╗║║╔╗╔╗║
    ─║║║║─╚╗╔╝─║║─║║║║─║║║║─╚╝║║─║║║╚══╗╚╝║║╚╝
    ─║║║║─╔╝╚╗─║╚═╝║║║─║║║║╔═╗║║─║║╚══╗║──║║──
    ╔╝╚╝║╔╝╔╗╚╗║╔═╗║║╚═╝║║╚╩═║║╚═╝║║╚═╝║──║║──
    ╚═══╝╚═╝╚═╝╚╝─╚╝╚═══╝╚═══╝╚═══╝╚═══╝──╚╝──

    Written by dxAugust (aka fet1sov)
*/

namespace DonationIntegration
{
    class DAPI
    {

        /* User show data request */
        public class Data
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string avatar { get; set; }
            public string email { get; set; }
            public string socket_connection_token { get; set; }
        }

        public class UserDataResponse
        {
            public Data data { get; set; }
        }
        /* ==================== */

        /* Access token request */
        public class AccessResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }
        /* =================== */

        /* Web socket auth - WEBSOCKET */
        public class Result
        {
            public string client { get; set; }
            public string version { get; set; }
        }

        public class WebSocketAuth   // Class for authentification JSON response
        {
            public int id { get; set; }
            public Result result { get; set; }
        }
        /* =============== */

        /* Channel subscribtion response */
        public class Channel
        {
            public string channel { get; set; }
            public string token { get; set; }
        }

        public class SubscribeDataResponse
        {
            public List<Channel> channels { get; set; }
        }
        /* =========================== */

        /* Successful channel connection Response - WEBSOCKET */
        public class ChannelData
        {
            public Info info { get; set; }
        }

        public class Info
        {
            public string user { get; set; }
            public string client { get; set; }
        }

        public class ChannelResult
        {
            public int type { get; set; }
            public string channel { get; set; }
            public Data data { get; set; }
        }

        public class ChannelDataResponse
        {
            public Result result { get; set; }
        }
        /* ====================================== */


        /* Donation info - WEBSOCKET */
        public class DonationData
        {
            public int seq { get; set; }
            public DonationUserData data { get; set; }
        }

        public class DonationUserData
        {
            public int id { get; set; }
            public string name { get; set; }
            public string username { get; set; }
            public string recipient_name { get; set; }
            public string message { get; set; }
            public string message_type { get; set; }
            public object payin_system { get; set; }
            public int amount { get; set; }
            public string currency { get; set; }
            public int is_shown { get; set; }
            public float amount_in_user_currency { get; set; }
            public string created_at { get; set; }
            public object shown_at { get; set; }
            public string reason { get; set; }
        }

        public class DonationResult
        {
            public string channel { get; set; }
            public DonationData data { get; set; }
        }

        public class DonationResponse
        {
            public DonationResult result { get; set; }
        }
        /* ========================= */

        private static bool subscribitionFlag = false;

        private static WebSocket webSocket;

        public DAPI()
        {
            webSocket = new WebSocket("wss://centrifugo.donationalerts.com/connection/websocket");
            webSocket.OnOpen += OnSocketConnect;
            webSocket.OnClose += OnSocketDisconnect;
            webSocket.OnError += OnErrorSocket;
            webSocket.OnMessage += OnSocketMessage;
            webSocket.Connect();

            /* All authentification is here */
            Config.access_token = getAccessToken(Config.authCode);
            webSocket.Send($"{{\"params\":{{\"token\":\"{getSocketToken()}\"}},\"id\":1}}");
            /* =========================== */

            void OnSocketConnect(object sender, EventArgs evt)
            {
                Debugger.successOutput("Sucessfully connected to DA server");
            }

            void OnSocketDisconnect(object sender, CloseEventArgs evt)
            {
                Debugger.errorOutput("Disconnected from DA server");
            }

            void OnErrorSocket(object sender, WebSocketSharp.ErrorEventArgs evt)
            {
                Debugger.errorOutput("Received Error");
            }
        }

        private static string getAccessToken(string authToken)
        {
            string accessToken = "";

            var url = "https://www.donationalerts.com/oauth/token";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.ContentType = "application/x-www-form-urlencoded";

            var data = "grant_type=authorization_code&client_id=" + Config.client_id + "&client_secret=" + Config.client_secret + "&redirect_url=" + Config.redirect_URL + "&code=" + Config.authCode;

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    AccessResponse accessResponse = JsonConvert.DeserializeObject<AccessResponse>(result);

                    accessToken = accessResponse.access_token;
                    Config.refresh_token = accessResponse.refresh_token;

                    // Делаем запоминание refreshToken'а, для последующего обновления
                    string configPath = Path.Combine(Directory.GetCurrentDirectory(), "ServerPlugins/DAIntegration/config.ini");
                    string configDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ServerPlugins/DAIntegration");

                    if (Directory.Exists(@configDirectory))
                    {
                        if (File.Exists(@configPath))
                        {
                            IniParser parser = new IniParser(@configPath);
                            parser.AddSetting("DonationAlerts", "refreshtoken", accessResponse.refresh_token);
                        }
                    }

                    Debugger.successOutput("Successfully got the Access Token");
                }
                else
                {
                    Debugger.errorOutput("ERROR! AUTH CODE EXPIRED");
                }
            }

            return accessToken;
        }

        private static string getSocketToken()
        {
            string socketToken = "";

            var url = "https://www.donationalerts.com/api/v1/user/oauth";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Headers["Authorization"] = "Bearer " + Config.access_token;

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                UserDataResponse userDataResponse = JsonConvert.DeserializeObject<UserDataResponse>(result);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    UserDataResponse userResponse = JsonConvert.DeserializeObject<UserDataResponse>(result);

                    socketToken = userResponse.data.socket_connection_token;
                    Config.channel_id = userResponse.data.id.ToString();

                    Debugger.successOutput("Successfully got the Socket Token");
                }
                else
                {
                    Debugger.errorOutput("ERROR! AUTH CODE EXPIRED");
                }
            }

            return socketToken;
        }
        private static void subscribeChannel()
        {
            var url = "https://www.donationalerts.com/api/v1/centrifuge/subscribe";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.ContentType = "application/json";

            var data = "{\"channels\":[\"$alerts:donation_" + Config.channel_id + "\"], \"client\":\"" + Config.socketClient_id + "\"}";

            httpRequest.ContentType = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer " + Config.access_token;

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    SubscribeDataResponse subDataResponse = JsonConvert.DeserializeObject<SubscribeDataResponse>(result);

                    Config.channelConnection_token = subDataResponse.channels[0].token;

                    webSocket.Send($"{{\"params\":{{\"channel\":\"$alerts:donation_{Config.channel_id}\",\"token\":\"{Config.channelConnection_token}\"}},\"method\":1,\"id\":2}}");

                    Debugger.successOutput("Successfully got the channel connection token");
                }
                else
                {
                    Debugger.errorOutput("ERROR! CAN`T CONNECT TO CHANNEL");
                }
            }
        }

        private static void OnSocketMessage(object sender, MessageEventArgs evt)
        {
            if (Config.bDebug)
            {
                Debugger.messageOutput("Message from socket: " + evt.Data);
            }

            if (evt.Data.StartsWith("{\"id\":1"))
            {
                WebSocketAuth socketResponse = JsonConvert.DeserializeObject<WebSocketAuth>(evt.Data);

                Config.socketClient_id = socketResponse.result.client;

                subscribeChannel();
            }

            if (subscribitionFlag)
            {
                Debugger.successOutput("Successfully got subscribed to the channel");

                webSocket.Send($"{{\"params\":{{\"channel\":\"$alerts:donation_{Config.channel_id}\",\"token\":\"{Config.channelConnection_token}\"}},\"method\":1,\"id\":2}}");

                subscribitionFlag = false;
            }

            /* Here we sending information to Plugin and then work in Terraria */
            if (evt.Data.StartsWith("{\"result\":{\"channel\":\"$alerts:donation"))
            {
                DonationResponse donationResponse = JsonConvert.DeserializeObject<DonationResponse>(evt.Data);
                Plugin.handleDonate(donationResponse.result.data.data.username, donationResponse.result.data.data.currency, donationResponse.result.data.data.amount);
            }
        }
    }
}