using Discord.WebSocket;

namespace DiscordBot.util;

public static class PatreonSolver {
    private static readonly List<ulong> Roles = new() { 0, 1281559477526597694, 1281559640483565603, 1281559728970793010, 1281559778224508969, 0, 0, 0, 0, 1278159074697019423, 1281790860756914208, 1252962987464196186 };

    public static Patreon Resolve(SocketGuildUser user) {
        for (int i = Roles.Count - 1; i > 0; i--) {
            ulong role = Roles[i];
            if (user.Roles.Any(x => x.Id == role)) return (Patreon)i;
        }
        return Patreon.None;
    }

    public enum Patreon {
        None, Coal, Iron, Gold, Diamond, Obsidian, Netherite, Bedrock, Void, Helper, Contributor, Owner
    }
}
