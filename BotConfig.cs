using DiscordBot.util;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordBot;

public class BotConfig : Config {
    [JsonInclude]
    [JsonPropertyName("token")]
    public string Token = "";
    [JsonInclude]
    [JsonPropertyName("sponserFile")]
    public string SponserFilePath = "";

    public BotConfig() : base("") { }

    public BotConfig(string path) : base(path) { }

    public override void ForceSave() {
        StreamWriter sw = new(ConfigPath, false);
        sw.Write(JsonSerializer.Serialize(this));
        sw.Close();
    }

    public override void Load() {
        StreamReader sr = new(ConfigPath);
        BotConfig another = JsonSerializer.Deserialize<BotConfig>(sr.ReadToEnd()) ?? new BotConfig();
        CopyFrom(another);
        sr.Close();
    }

    private void CopyFrom(BotConfig another) {
        Token = another.Token;
        SponserFilePath = another.SponserFilePath;
    }
}
