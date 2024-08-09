using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Класс для загрузки и сохранения конфигурации приложения.
/// </summary>
public static class ConfigLoader
{
    /// <summary>
    /// Загружает конфигурацию из указанного файла. Если файл не существует, то выставляются настройки по умолчанию.
    /// </summary>
    /// <param name="path">Путь к файлу конфигурации.</param>
    /// <returns>Конфигурация приложения.</returns>
    public static AppConfig LoadConfig(string path)
    {
        if (!File.Exists(path))
        {
            var defaultConfig = new AppConfig
            {
                UpdateIntervalSeconds = 30, // Значение по умолчанию (если нет)
                ProxySettings = null
            };
            SaveConfig(path, defaultConfig);
        }

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<AppConfig>(json);
    }

    /// <summary>
    /// Сохраняет конфигурацию приложения.
    /// </summary>
    /// <param name="path">Путь к файлу, куда будет сохранятся текущая конфигурация.</param>
    /// <param name="config">Конфигурация приложения.</param>
    public static void SaveConfig(string path, AppConfig config)
    {
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}
