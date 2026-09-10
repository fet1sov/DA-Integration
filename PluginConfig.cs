using System.Text.Json.Serialization;
using DA_Integration.Models;

namespace DA_Integration
{
    public class PluginConfig
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "";

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = "";

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; } = "http://localhost";

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = "";

        [JsonPropertyName("debug")]
        public bool Debug { get; set; } = false;

        [JsonPropertyName("events")]
        public List<DonateEventConfig> Events { get; set; } = new List<DonateEventConfig>();
    }
}