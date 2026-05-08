using System.Text.Json;

namespace StatistiquesHGG.UI;

public static class AppSettings
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    private static AppConfig _config = new();

    static AppSettings()
    {
        Load();
    }

    public static string ConnectionString => _config.ConnectionString;
    public static string ReportsFolder => _config.ReportsFolder;
    public static string BackupFolder => _config.BackupFolder;

    private static void Load()
    {
        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        else
        {
            _config = new AppConfig();
            Save();
        }
    }

    public static void Save()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    public static void UpdateConnectionString(string cs)
    {
        _config.ConnectionString = cs;
        Save();
    }
}

public class AppConfig
{
    public string ConnectionString { get; set; } =
        "Server=localhost;Port=3306;Database=hgg_statistiques;User=root;Password=;CharSet=utf8mb4;";
    public string ReportsFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HGG_Rapports");
    public string BackupFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HGG_Sauvegardes");
}
