using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.Conditions;

namespace Silk.Commands;

public class ThrowCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    public ThrowCommand(ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _context       = context;
        _channels = channels;
    }

    [Command("throw")]
    [RequireTeamOrOwner]
    [Description("A command mainly used for debugging.")]
    public Task<IResult> ThrowAsync() => throw new();

    [Command("multi-image")]
    [RequireTeamOrOwner]
    public async Task<IResult> MultiImageAsync(int embedCount)
    {
        if (embedCount is < 0 or > 10)
            return await _channels.CreateMessageAsync(_context.ChannelID, "Invalid number of embeds.");

        var embeds = Enumerable.Range(0, embedCount)
                               .Select
                                    (
                                     e => new Embed
                                     {
                                         Title = $"{e / 4}/{embedCount }",
                                         Url   = $"https://example-{e / 4}.com",
                                         Image = new EmbedImage("https://cdn.velvetthepanda.dev/l0FFjgo2Ez.png")
                                     }
                                    )
                               .ToArray();
        
        return await _channels.CreateMessageAsync(_context.ChannelID, embeds: embeds);
    }
    
}