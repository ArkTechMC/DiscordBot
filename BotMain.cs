using Discord;
using Discord.WebSocket;
using DiscordBot.util;
using System.Globalization;
using Timer = System.Timers.Timer;

namespace DiscordBot;

public class BotMain {
    public static readonly BotMain Instance = new();
    private readonly BotConfig _config = new(Environment.CurrentDirectory + "/main.json");
    private readonly DiscordSocketClient _client;
    private readonly PatreonConfig _patreonConfig;

    private BotMain() {
        _client = new DiscordSocketClient(new DiscordSocketConfig {
            AlwaysDownloadUsers = true
        });
        _client.Log += Log;
        _client.SlashCommandExecuted += OnSlashCommand;
        _client.Connected += OnConnect;
        _client.GuildMemberUpdated += OnGuildMemberUpdated;
        _patreonConfig = new PatreonConfig(_config.SponsorFilePath);
        Timer timer = new(60 * 60 * 1000);
        timer.Elapsed += (sender, e) => {
            Logger.Info("Checking Permissions");
            SocketGuild guild = _client.GetGuild(1252840735087132794);
            foreach (ulong user in _patreonConfig.All())
                _patreonConfig.GetOrCreate(guild.GetUser(user), user);
        };
        timer.AutoReset = true;
        timer.Start();
    }

    public async Task Launch() {
        Console.WriteLine("Launching Bot...");
        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private async Task OnConnect() {
        await RegisterSlashCommand();
    }

    private Task OnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> oldUser, SocketGuildUser newUser) {
        PatreonConfig.PatreonInfo info = _patreonConfig.GetOrCreate(newUser, newUser.Id);
        info.Patreon = PatreonSolver.Resolve(newUser);
        Logger.Info($"[{DateTime.Now.ToString(CultureInfo.CurrentCulture)}] {newUser.Username}: Role changed: {info.Patreon.ToString()}");
        _patreonConfig.Save();
        return Task.CompletedTask;
    }

    private async Task RegisterSlashCommand() {
        await _client.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
            .WithName("bind")
            .WithDescription("Bind your Minecraft Account")
            .WithDefaultPermission(true)
            .AddOption("ign", ApplicationCommandOptionType.String, "Minecraft Account Name", true).Build());
    }

    private async Task OnSlashCommand(SocketSlashCommand command) {
        if (command.User is not SocketGuildUser socketGuildUser) return;
        if (command.CommandName == "bind") {
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.CurrentCulture)}] {socketGuildUser.Username}: {command.CommandName}");
            MojangApi.MojangApiResponse response = MojangApi.GetMojangInfo((string)command.Data.Options.First().Value);
            if (response.Id == "")
                await command.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Failed to bind account!")
                    .WithDescription($"Reason: {response.Name}")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build(), ephemeral: true);
            else {
                PatreonConfig.PatreonInfo info = _patreonConfig.GetOrCreate(socketGuildUser, socketGuildUser.Id);
                info.McUuid = response.Id;
                info.NickName = response.Name;
                _patreonConfig.Save();
                await command.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Successfully bind account!")
                    .WithDescription($"Name: `{response.Name}`\nUUID: `{response.Id}`\nYour Sponsorship: `{info.Patreon.ToString()}`")
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .Build(), ephemeral: true);
            }
        }
    }

    private static Task Log(LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
