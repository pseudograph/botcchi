using DSharpPlus.CommandsNext;
using System;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Botcchi.Commands;

public class MiscCommands : ApplicationCommandModule
{
    [SlashCommand("ping", "responds with ping")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Pong"));
    }
}