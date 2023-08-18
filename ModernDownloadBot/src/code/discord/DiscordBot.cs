using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

public class DiscordBot
{
    public DiscordSocketClient client;
    public string token;

    private CommandService commands;

    public DiscordBot(string _discordToken)
    {
        token = _discordToken;
    }

    public async Task RunBotAsync()
    {
        DiscordSocketConfig config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };

        client = new DiscordSocketClient(config);

        commands = new CommandService();

        client.Ready += async() =>
        {
            
            var channel = client.GetChannel(Program.settings.configuration.channelId) as SocketTextChannel;
            
            var botAdmin = client.GetUser(Program.settings.configuration.botAdminId);

            if (channel == null)
            {
                Console.WriteLine("The channel Id is invalid!");
                //ajouter le code pour modifier channel Id
            }
            else
            {
                ulong botMention = Program.settings.configuration.otherBotId;
                Console.WriteLine("Gathering Welcome message...");
                string welcomeMessage = await Program.GetGithubText("https://raw.githubusercontent.com/Remi-Dv/informations/main/DownloadDiscordBot/startMessage.txt");
                await channel.SendMessageAsync(welcomeMessage);
                Console.WriteLine();
                Console.WriteLine("Checking for updates...");
                await channel.SendMessageAsync(await Program.CheckNewUpdates());
                Console.WriteLine();
                if (botAdmin != null)
                {
                    var mention = botAdmin.Mention;
                    await channel.SendMessageAsync($"The bot started! {mention} is my admin, " +
                        $"and <@{botMention}> is the other bot. !Help for more infos.");
                }
                else
                {
                    await channel.SendMessageAsync($"The bot started! However the admin Id is not valid. !Help for more infos.");
                }
            }

            Console.WriteLine($"The bot {client.CurrentUser} has successfully started");
        };

        await InstallCommandsAsync();

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await Task.Delay(-1);
    }

    public async Task InstallCommandsAsync()
    {
        client.MessageReceived += HandleCommandAsync;
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
    }

    private async Task HandleCommandAsync(SocketMessage pMessage)
    {
        var message = pMessage as SocketUserMessage;

        if (message == null)
        {
            return;
        }

        int argPos = 0;

        if (!message.HasCharPrefix('!', ref argPos) || message.Author.Id == client.CurrentUser.Id)
        {
            return;
        }

        var context = new SocketCommandContext(client, message);

        var result = await commands.ExecuteAsync(context, argPos, null);

        if (!result.IsSuccess)
        {
            if (result.Error == CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync("Unknown Command !Help for more informations");
            }
            else
            {
                await context.Channel.SendMessageAsync("Error :" + result.ErrorReason);
            }
        }
    }
}