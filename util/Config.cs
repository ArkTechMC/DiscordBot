using System.Text.Json.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DiscordBot.util;

public abstract class Config {
    private const int MaxUnsaveCount = 10;
    protected static readonly List<Config> Configs = new();
    [JsonIgnore]
    protected readonly string ConfigPath;
    [JsonIgnore]
    private int _saveCount;
    [JsonIgnore]
    private bool _shouldSave;

    static Config() {
        Timer timer = new(60 * 1000);
        timer.Elapsed += SaveAllConfig;
        timer.AutoReset = true;
        timer.Start();
    }

    protected Config(string path) {
        ConfigPath = path;
        if (path == "") return;
        if (File.Exists(ConfigPath)) {
            Logger.Info($"正在加载 {ConfigPath}");
            Load();
        }
        else
            Logger.Warn($"未找到文件 {ConfigPath}");
    }

    private static void SaveAllConfig(object? sender, ElapsedEventArgs e) {
        lock (Configs) {
            Logger.Info("Checking Unsaved Configs");
            foreach (Config config in Configs.Where(config => config._shouldSave)) {
                Logger.Info($"正在保存{config.ConfigPath}");
                config.ForceSave();
                config._saveCount = 0;
                config._shouldSave = false;
            }
        }
    }

    public void Save() {
        _shouldSave = true;
        _saveCount++;
        if (_saveCount < MaxUnsaveCount) return;
        Logger.Info($"正在保存 {ConfigPath}");
        ForceSave();
        _saveCount = 0;
        _shouldSave = false;
    }

    public abstract void ForceSave();

    public abstract void Load();
}
