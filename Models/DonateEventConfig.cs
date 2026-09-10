using System.Text.Json;
using System.Text.Json.Serialization;

namespace DA_Integration.Models
{
    public class DonateEventConfig
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DonateEventType Type { get; set; }

        [JsonPropertyName("min_amount")]
        public int? MinAmount { get; set; }

        [JsonPropertyName("probability")]
        public double Probability { get; set; } = 100.0;

        [JsonPropertyName("npc_id")]
        public int? NpcId { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("projectile_id")]
        public int? ProjectileId { get; set; }

        [JsonPropertyName("damage")]
        public int? Damage { get; set; }

        [JsonPropertyName("radius")]
        public int? TeleportRadius { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class DonateEventConfigConverter : JsonConverter<DonateEventConfig>
    {
        public override DonateEventConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var config = new DonateEventConfig();

            if (root.TryGetProperty("type", out var typeProp) && Enum.TryParse<DonateEventType>(typeProp.GetString(), true, out var eventType))
            {
                config.Type = eventType;
            }

            if (root.TryGetProperty("min_amount", out var minAmountProp) && minAmountProp.ValueKind != JsonValueKind.Null)
                config.MinAmount = minAmountProp.GetInt32();

            if (root.TryGetProperty("probability", out var probProp) && probProp.ValueKind != JsonValueKind.Null)
                config.Probability = probProp.GetDouble();

            if (root.TryGetProperty("npc_id", out var npcProp) && npcProp.ValueKind != JsonValueKind.Null)
                config.NpcId = npcProp.GetInt32();

            if (root.TryGetProperty("count", out var countProp) && countProp.ValueKind != JsonValueKind.Null)
                config.Count = countProp.GetInt32();

            if (root.TryGetProperty("projectile_id", out var projProp) && projProp.ValueKind != JsonValueKind.Null)
                config.ProjectileId = projProp.GetInt32();

            if (root.TryGetProperty("damage", out var dmgProp) && dmgProp.ValueKind != JsonValueKind.Null)
                config.Damage = dmgProp.GetInt32();

            if (root.TryGetProperty("radius", out var radProp) && radProp.ValueKind != JsonValueKind.Null)
                config.TeleportRadius = radProp.GetInt32();

            return config;
        }

        public override void Write(Utf8JsonWriter writer, DonateEventConfig value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}