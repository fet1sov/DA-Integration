using DA_Integration;
using System;
using System.IO;
using System.Text.Json;
using TShockAPI;

namespace DA_Integration
{
    public static class ConfigManager
    {
        public static readonly string FolderPath = Path.Combine(TShock.SavePath, "DAIntegration");
        public static readonly string FilePath = Path.Combine(FolderPath, "config.json");

        public static PluginConfig Config { get; private set; } = new PluginConfig();

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
                    Config = new PluginConfig();
                    Save();
                    TShock.Log.ConsoleWarn("[DAIntegration] Файл конфигурации не найден и был создан по умолчанию. Укажите access_token и refresh_token.");
                }
                else
                {
                    string json = File.ReadAllText(FilePath);
                    Config = JsonSerializer.Deserialize<PluginConfig>(json) ?? new PluginConfig();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[DAIntegration] Ошибка при загрузке конфига: {ex.Message}");
                Config = new PluginConfig();
            }
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Config, options);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[DAIntegration] Ошибка при сохранении конфига: {ex.Message}");
            }
        }
    }
}