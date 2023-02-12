using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
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
        if (context is not MessageContext mc)
        {
            return Result.FromSuccess();
        }

        if (commandResult is not Result<ReactionResult> re)
        {
            return Result.FromSuccess();
        }
        
        await _channels.CreateReactionAsync
        (
         mc.Message.ChannelID.Value,
         mc.Message.ID.Value,
         (re.Entity ?? re.Error as ReactionResult)!.Reaction.TryPickT0(out Snowflake snowflake, out var ulongOrRaw) 
             ? $"_:{snowflake}" 
             : ulongOrRaw.TryPickT0(out var id, out var unicode) 
                 ? $"_:{id}" 
                 : unicode,
         ct
        );

        if ((re.Entity ?? re.Error as ReactionResult)!.Message.IsDefined(out var message))
        {
            if (!context.TryGetChannelID(out var cid))
            {
                return Result.FromSuccess();
            }
            
            await _channels.CreateMessageAsync(cid.Value, message, ct: ct);
        }
        
        return Result.FromSuccess();
    }
}