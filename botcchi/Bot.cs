using System.Text;
using Botcchi.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DSharpPlus.SlashCommands;
using JikanDotNet;

namespace Botcchi;

public class Bot
{
    public DiscordClient Client { get; private set; }
    public CommandsNextExtension Commands { get; private set; }

    public Bot()
    {
        var json = string.Empty;
        using (var fs = File.OpenRead("config.json"))
        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            json = sr.ReadToEnd();

        var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
        
        var config = new DiscordConfiguration
        {
            Token = configJson.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug,
        };
        
        Client = new DiscordClient(config);
        Client.Ready += OnClientReady;
        
        var slash = Client.UseSlashCommands();

        slash.RegisterCommands<MiscCommands>(Consts.TestGuild);
        slash.RegisterCommands<AnimeCommands>(Consts.TestGuild);
    }

    private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
    {
        return Task.CompletedTask;
    }

    public async Task Run()
    {
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }
}