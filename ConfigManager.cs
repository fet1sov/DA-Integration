using System.Text.Json;
using System.Text.Json.Serialization;
using TShockAPI;
using DA_Integration.Models;

namespace DA_Integration
{
    public static class ConfigManager
    {
        public static readonly string FolderPath = Path.Combine(TShock.SavePath, "DAIntegration");
        public static readonly string FilePath = Path.Combine(FolderPath, "config.json");

        public static PluginConfig Config { get; private set; } = new PluginConfig();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void LoadOrCreate()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                if (!File.Exists(FilePath))
                {
                    Config = CreateDefaultConfig();
                    Save();
                    TShock.Log.ConsoleWarn("[DAIntegration] Файл конфигурации создан с базовыми примерами.");
                }
                else
                {
                    string json = File.ReadAllText(FilePath);
                    Config = JsonSerializer.Deserialize<PluginConfig>(json, JsonOptions) ?? CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[DAIntegration] Ошибка при загрузке конфига: {ex.Message}");
                Config = CreateDefaultConfig();
            }
        }

        public static void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Config, JsonOptions);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[DAIntegration] Ошибка при сохранении конфига: {ex.Message}");
            }
        }

        private static PluginConfig CreateDefaultConfig()
        {
            return new PluginConfig
            {
                ClientId = "",
                ClientSecret = "",
                RedirectUrl = "http://localhost",
                AccessToken = "",
                RefreshToken = "",
                Debug = false,
                Events = new List<DonateEventConfig>
                {
                    new DonateEventConfig { Type = DonateEventType.SPAWN_BOSS, MinAmount = 100, Probability = 15.0, NpcId = 4 },
                    new DonateEventConfig { Type = DonateEventType.SPAWN_MOBS, MinAmount = 50, Probability = 30.0, NpcId = 3, Count = 15 },
                    new DonateEventConfig { Type = DonateEventType.KILL_ALL_EVENT, MinAmount = 500, Probability = 5.0 },
                    new DonateEventConfig { Type = DonateEventType.KILL_RANDOM_PLAYER, MinAmount = 75, Probability = 20.0 },
                    new DonateEventConfig { Type = DonateEventType.SPAWN_BOMB_UNDER_PLAYER, MinAmount = 25, Probability = 40.0, Damage = 60, ProjectileId = 16 },
                    new DonateEventConfig { Type = DonateEventType.SPAWN_DYNAMITE_UNDER_PLAYER, MinAmount = 150, Probability = 10.0, Damage = 250, ProjectileId = 29 },
                    new DonateEventConfig { Type = DonateEventType.TELEPORT_PLAYER_IN_RANDOM, MinAmount = 30, Probability = 50.0, TeleportRadius = 30 },
                    new DonateEventConfig { Type = DonateEventType.DAMAGE_BY_STAND_BLOCK, Probability = 100.0, Damage = 40 }
                }
            };
        }
    }
}