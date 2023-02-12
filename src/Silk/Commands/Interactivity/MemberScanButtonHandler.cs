using System.Collections.Generic;
using System.Linq;
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
using Remora.Discord.Caching.Abstractions;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Rest.Core;
using RemoraViewsPOC.Extensions;
using Silk.Utilities;
using Silk.Views;

namespace Silk.Commands.Interactivity;

[SuppressInteractionResponse(true)]
[RequireDiscordPermission(DiscordPermission.KickMembers, DiscordPermission.BanMembers)]
public class MemberScanButtonHandler : InteractionGroup
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
    
    //member-check:dump | member-check:kick | member-check:ban

    [Button("member-check::dump")]
    public async Task<Result> DumpAsync()
    {
        var key = CacheKey.StringKey($"Silk:SuspiciousMemberCheck:{_context.Interaction.GuildID.Value}:Members");
        
        var idCheck = await _cache.TryGetValueAsync<IReadOnlyList<Snowflake>>(key, CancellationToken);
        
        if (!idCheck.IsDefined(out var IDs))
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "It seems the IDs have gone missing! This is likely due to a service restart.",
             flags: MessageFlags.Ephemeral,
             ct: CancellationToken
            );
        
        var file = IDs.Join(" ").AsStream();
            
        return (Result)await _interactions.CreateFollowupMessageAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         "Here you go!",
         flags: MessageFlags.Ephemeral,
         attachments: new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("IDs.txt", file)) },
         ct: CancellationToken
        );
    }

    [Button("member-check::kick")]
    public async Task<Result> KickAsync()
    {
        var key = CacheKey.StringKey($"Silk:SuspiciousMemberCheck:{_context.Interaction.GuildID.Value}:Members");
        
        var idCheck = await _cache.TryGetValueAsync<IReadOnlyList<Snowflake>>(key, CancellationToken);
        
        var components = ((IPartialActionRowComponent)_context.Interaction.Message.Value.Components.Value[0]).Components.Value;
        
        if (!idCheck.IsDefined(out var IDs))
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "It seems the IDs have gone missing! This is likely due to a service restart.",
             flags: MessageFlags.Ephemeral,
             ct: CancellationToken
            );
        
        await _interactions.EditOriginalInteractionResponseAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         new MemberScanView(true),
         CancellationToken
        );

        var followupResult = await _interactions.CreateFollowupMessageAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         "Alright! This could take a while.",
         flags: MessageFlags.Ephemeral,
         ct: CancellationToken
        );

        if (!followupResult.IsDefined(out var followup))
            return (Result)followupResult;
            
        var kicked = await Task.WhenAll(IDs.Select(id => _infractions.KickAsync(_context.Interaction.GuildID.Value, id, _context.GetUserID(), "Phishing detected: Moderater initiated manual mass-kick.", false)));
            
        var failed = kicked.Count(r => !r.IsSuccess);
            
        return (Result)await _interactions.EditFollowupMessageAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         followup.ID,
         $"Done! Kicked {IDs.Count - failed}/{IDs.Count} users.",
         ct: CancellationToken
        );
    }

    [Button("member-check::ban")]
    public async Task<Result> BanAsync()
    {
        var key = CacheKey.StringKey($"Silk:SuspiciousMemberCheck:{_context.Interaction.GuildID.Value}:Members");
        var idCheck = await _cache.TryGetValueAsync<IReadOnlyList<Snowflake>>(key, CancellationToken);

        if (!idCheck.IsDefined(out var IDs))
            return (Result)await _interactions.CreateFollowupMessageAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "It seems the IDs have gone missing! This is likely due to a service restart.",
             flags: MessageFlags.Ephemeral,
             ct: CancellationToken
            );
        
        await _interactions.EditOriginalInteractionResponseAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         new MemberScanView(true),
         ct: CancellationToken
        );
            
        var followupResult = await _interactions.CreateFollowupMessageAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         "Alright! This could take a while.",
         flags: MessageFlags.Ephemeral,
         ct: CancellationToken
        );

        if (!followupResult.IsDefined(out var followup))
            return (Result)followupResult;

        var kicked = await Task.WhenAll(IDs.Select(id => _infractions.BanAsync(_context.Interaction.GuildID.Value, id, _context.GetUserID(), 0, "Phishing detected: Moderater initiated manual mass-kick.", notify: false)));
            
        var failed = kicked.Count(r => !r.IsSuccess);
            
        return (Result)await _interactions.EditFollowupMessageAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         followup!.ID,
         $"Done! Banned {IDs.Count - failed}/{IDs.Count} users.",
         ct: CancellationToken
        );
    }
}