using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Shared;

public class PostCommandReactionHandler : IPostExecutionEvent
{
    private readonly IDiscordRestChannelAPI _channels;
    public PostCommandReactionHandler(IDiscordRestChannelAPI channels) => _channels = channels;

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default)
    {
        if (!commandResult.IsSuccess)
        {
            return Result.FromSuccess();
        }

        if (context is not MessageContext mc)
        {
            return Result.FromSuccess();
        }

        if (commandResult.Inner!.Inner!.Inner!.Inner is not Result<ReactionResult> re)
        {
            return Result.FromSuccess();   
        }
        
        await _channels.CreateReactionAsync
        (
         context.ChannelID,
         mc.MessageID,
         re.Entity.Reaction.TryPickT0(out Snowflake snowflake, out var ulongOrRaw) 
             ? $"_:{snowflake}" 
             : ulongOrRaw.TryPickT0(out var id, out var unicode) 
                 ? $"_:{id}" 
                 : unicode,
         ct
        );

        if (re.Entity.Message.IsDefined(out var message))
        {
            await _channels.CreateMessageAsync(context.ChannelID, message, ct: ct);
        }
        
        return Result.FromSuccess();
    }
}