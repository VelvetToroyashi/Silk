using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Silk.Commands.Conditions;

/// <summary>
///     Represents that a channel should be marked as NSFW.
/// </summary>
public class RequireNSFWCondition : ICondition<NSFWChannelAttribute>
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channelApi;
    public RequireNSFWCondition(ICommandContext context, IDiscordRestChannelAPI channelApi)
    {
        _context    = context;
        _channelApi = channelApi;
    }

    public async ValueTask<Result> CheckAsync(NSFWChannelAttribute attribute, CancellationToken ct = default)
    {
        var channelID = _context switch
        {
            IInteractionCommandContext interactionContext => interactionContext.Interaction.ChannelID.Value,
            ITextCommandContext messageContext     => messageContext.Message.ChannelID.Value,
        };
        
        Result<IChannel> channelRes = await _channelApi.GetChannelAsync(channelID, ct);

        if (!channelRes.IsSuccess)
            return Result.FromError(channelRes.Error);

        IChannel channel = channelRes.Entity;

        if (channel.IsNsfw.IsDefined(out bool nsfw) && nsfw)
            return Result.FromSuccess();

        if (channel.Type is ChannelType.DM)
            return Result.FromSuccess(); // DMs are always NSFW ;3

        return Result.FromError(new InvalidOperationError("This channel is not NSFW."));
    }
}