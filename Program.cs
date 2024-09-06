namespace DiscordBot;

public static class Program {
    public static Task Main() {
        return BotMain.Instance.Launch();
    }
}
