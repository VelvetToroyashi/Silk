using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;
using Silk.Extensions;
using Silk.Services.Interfaces;
using OneOf;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Silk.Interactivity;

public class MemberScanButtonHandler : IButtonInteractiveEntity
{
    
    private readonly CacheService               _cache;
    private readonly InteractionContext         _context;
    private readonly IInfractionService         _infractions;
    private readonly IDiscordRestGuildAPI       _guilds;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public MemberScanButtonHandler
    (
        CacheService         cache,
        InteractionContext   context,
        IInfractionService   infractions,
        IDiscordRestGuildAPI guilds,
        IDiscordRestInteractionAPI interactions
    )
    {
        _cache        = cache;
        _context      = context;
        _infractions  = infractions;
        _interactions = interactions;
        _guilds  = guilds;
    }


    public Task<Result<bool>> IsInterestedAsync(ComponentType? componentType, string customID, CancellationToken ct = default)
        => Task.FromResult(Result<bool>.FromSuccess(customID.StartsWith("member-check")));

    public async Task<Result> HandleInteractionAsync(IUser user, string customID, CancellationToken ct = default)
    {
        var action      = customID.Split(':')[1];
        var guildID     = _context.GuildID.Value;
        var permissions = _context.Member.Value.Permissions.Value;
        var components  = (_context.Message.Value.Components.Value[0] as IPartialActionRowComponent)!.Components.Value;
        
        // Don't worry, I hate this too :meowpain:
        if ((!permissions.HasPermission(DiscordPermission.KickMembers) ||
             !permissions.HasPermission(DiscordPermission.BanMembers)) &&
             !permissions.HasPermission(DiscordPermission.Administrator))
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "Sorry, but you're missing permissions to use this!",
             flags: MessageFlags.Ephemeral,
             ct: ct
            );
        
        var idCheck = await _cache.TryGetValueAsync<IReadOnlyList<Snowflake>>($"Silk:SuspiciousMemberCheck:{guildID}:Members", ct);
        
        if (!idCheck.IsDefined(out var IDs))
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "It seems the IDs have gone missing! This is likely due to a service restart.",
             flags: MessageFlags.Ephemeral,
             ct: ct
            );

        if (action is "dump")
        {
            var file = IDs.Join(" ").AsStream();
            
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "Here you go!",
             flags: MessageFlags.Ephemeral,
             attachments: new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("IDs.txt", file)) },
             ct: ct
            );
        }

        if (action is "kick")
        {
            await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             components: new[]
             {
                 new ActionRowComponent(new[] { AsButton(components[0], false), AsButton(components[1], true), AsButton(components[2], false) })
             },
             ct: ct
            );
            
            var followupResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "Alright! This could take a while.",
             flags: MessageFlags.Ephemeral,
             ct: ct
            );

            if (followupResult.IsDefined(out var followup))
                return (Result)followupResult;
            
            var kicked = await Task.WhenAll(IDs.Select(id => _infractions.KickAsync(guildID, id, user.ID, "Phishing detected: Moderater initiated manual mass-kick.", false)));
            
            var failed = kicked.Count(r => !r.IsSuccess);
            
            return (Result)await _interactions.EditFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             followup!.ID,
             $"Done! Kicked {IDs.Count - failed}/{IDs.Count} users.",
             ct: ct
            );
        }
        
        if (action is "ban")
        {
            await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             components: new[]
             {
                 new ActionRowComponent(new[] { AsButton(components[0], false), AsButton(components[1], true), AsButton(components[2], true) })
             },
             ct: ct
            );
            
            var followupResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "Alright! This could take a while.",
             flags: MessageFlags.Ephemeral,
             ct: ct
            );

            if (followupResult.IsDefined(out var followup))
                return (Result)followupResult;

            var kicked = await Task.WhenAll(IDs.Select(id => _infractions.BanAsync(guildID, id, user.ID, 0, "Phishing detected: Moderater initiated manual mass-kick.", notify: false)));
            
            var failed = kicked.Count(r => !r.IsSuccess);
            
            return (Result)await _interactions.EditFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             followup!.ID,
             $"Done! Banned {IDs.Count - failed}/{IDs.Count} users.",
             ct: ct
            );
        }

        ButtonComponent AsButton(IPartialMessageComponent component, bool disabled)
        {
            var button = component as IPartialButtonComponent;

            return new(button!.Style.Value, button.Label, button.Emoji, button.CustomID, IsDisabled: disabled);
        }
        
        return Result.FromSuccess();
    }
}