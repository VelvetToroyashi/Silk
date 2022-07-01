using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Interfaces;

namespace Silk.Interactivity;

public class JoinEmbedButtonHandler : InteractionGroup
{
    private readonly InteractionContext         _context;
    private readonly IInfractionService         _infractions;
    private readonly IDiscordRestUserAPI        _users;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public JoinEmbedButtonHandler(InteractionContext context, IInfractionService infractions, IDiscordRestUserAPI users, IDiscordRestInteractionAPI interactions)
    {
        _context      = context;
        _infractions  = infractions;
        _users        = users;
        _interactions = interactions;
    }
    
    [Button("join-action-kick")]
    [RequireDiscordPermission(DiscordPermission.KickMembers)]
    public async Task<Result> KickAsync()
    {
        _ = Snowflake.TryParse(_context.Message.Value.Embeds[0].Fields.Value[1].Value, out var userID);

        var infractionResult = await _infractions.KickAsync(_context.GuildID.Value, userID.Value, _context.User.ID, "Moderator-initiated action from join.");

        var logResult = await _interactions.CreateFollowupMessageAsync
        (
            _context.ApplicationID,
            _context.Token,
            infractionResult.IsSuccess ? $"Successfully kicked user." : infractionResult.Error.Message,
            flags: MessageFlags.Ephemeral
        );

        return (Result)logResult;
    }
    
    [Button("join-action-ban")]
    [RequireDiscordPermission(DiscordPermission.BanMembers)]
    public async Task<Result> BanAsync()
    {
        _ = Snowflake.TryParse(_context.Message.Value.Embeds[0].Fields.Value[1].Value, out var userID);
        
        var infractionResult = await _infractions.BanAsync(_context.GuildID.Value, userID.Value, _context.User.ID, 0, "Moderator-initiated action from join.");
        
        var logResult = await _interactions.CreateFollowupMessageAsync
        (
            _context.ApplicationID,
            _context.Token,
            infractionResult.IsSuccess ? $"Successfully banned user." : infractionResult.Error.Message,
            flags: MessageFlags.Ephemeral
        );
        
        return (Result)logResult;
    }
}