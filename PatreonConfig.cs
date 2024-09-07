using Discord.WebSocket;
using DiscordBot.util;
using System.Text.Json;

namespace DiscordBot;

public class PatreonConfig : Config {
    private List<PatreonInfo> _data = new();
    private Dictionary<ulong, PatreonInfo> _byId = new();

    public PatreonConfig(string path) : base(path) {
        Configs.Add(this);
    }

    public override void ForceSave() {
        StreamWriter sw = new(ConfigPath, false);
        sw.Write(JsonSerializer.Serialize(_data));
        sw.Close();
    }

    public override void Load() {
        StreamReader sr = new(ConfigPath);
        _data = JsonSerializer.Deserialize<List<PatreonInfo>>(sr.ReadToEnd()) ?? new List<PatreonInfo>();
        _byId = new Dictionary<ulong, PatreonInfo>();
        foreach (PatreonInfo info in _data)
            _byId.Add(info.DiscordId, info);
        sr.Close();
    }

    public PatreonInfo GetOrCreate(SocketGuildUser? user, ulong id) {
        if (!_byId.ContainsKey(id)) {
            PatreonInfo info = new() { DiscordId = id, Patreon = 0, McUuid = "", NickName = "" };
            _data.Add(info);
            _byId.Add(id, info);
        }
        PatreonInfo patreonInfo = _byId[id];
        patreonInfo.Patreon = user == null ? 0 : PatreonSolver.Resolve(user);
        Save();
        return patreonInfo;
    }

    public List<ulong> All() {
        return _byId.Keys.ToList();
    }

    public class PatreonInfo {
        public ulong DiscordId { get; init; }
        public PatreonSolver.Patreon Patreon { get; set; }
        public string McUuid { get; set; } = "";
        public string NickName { get; set; } = "";
    }
}
