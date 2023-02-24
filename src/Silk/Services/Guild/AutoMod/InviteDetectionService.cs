using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Services.Guild;

public class InviteDetectionService
{
   //TODO: Break this out into Regexes.cs?
   private static readonly Regex InviteRegex = new(@"discord\.gg\/(?<invite>[a-zA-Z0-9]+)", RegexOptions.Compiled); 
   
   private static readonly Regex AggressiveInviteRegex = new(@"(?:https?\:\/\/)?(www\.)?(((di?sc(?:ord)?\.(gg|io|me|li))|(discord(?:app)?\.com\/invite))\/(?<invite>[A-z0-9-]{2,}))", RegexOptions.Compiled);
  
   private readonly IMediator                       _mediator; 
   private readonly IInfractionService              _infractions;
   private readonly IDiscordRestUserAPI             _users;
   private readonly IDiscordRestInviteAPI           _invites;
   private readonly IDiscordRestChannelAPI          _channels;
   private readonly ExemptionEvaluationService      _exemptions;
   private readonly ILogger<InviteDetectionService> _logger;
   
   public InviteDetectionService
   (
      IMediator                       mediator,
      IInfractionService              infractions,
      IDiscordRestUserAPI             users,
      IDiscordRestInviteAPI           invites,
      IDiscordRestChannelAPI          channels,
      ExemptionEvaluationService      exemptions,
      ILogger<InviteDetectionService> logger
   )
   {
      _mediator    = mediator;
      _infractions = infractions;
      _users       = users;
      _invites     = invites;
      _channels    = channels;
      _exemptions  = exemptions;
      _logger      = logger;
   }
   
   /// <summary>
   /// Determines whether a given message contains an invite, and takes appropriate action.
   /// </summary>
   /// <param name="message">The message to check. Messages in DMs and empty messages will be ignored.</param>
   // The ONLY reason a result here is returned here is fluidity in the responder. It's a bit of a waste but meh.
   public async Task<Result> CheckForInviteAsync(IMessageCreate message)
   {
      if (!message.GuildID.IsDefined(out var guildID))
         return Result.FromSuccess(); // We don't process DM messages here. //
      
      if (string.IsNullOrEmpty(message.Content))
         return Result.FromSuccess();
      
      var config = await _mediator.Send(new GetGuildConfig.Request(guildID));
      var start  = DateTimeOffset.UtcNow;
      
      var inviteMatch = config.Invites.UseAggressiveRegex 
         ? AggressiveInviteRegex.Match(message.Content) 
         : InviteRegex.Match(message.Content);

      if (!inviteMatch.Success)
         return Result.FromSuccess();
      
      // We bail after regexing the message for metrics
      if (!config.Invites.WhitelistEnabled)
         return Result.FromSuccess();
      
      _logger.LogDebug(EventIds.AutoMod, "Detected invite in {Time:N0} ms (Regex: {RegexTime:N4} ms)", 
                       (message.ID.Timestamp - DateTimeOffset.UtcNow).TotalMilliseconds,
                       (DateTimeOffset.UtcNow - start).TotalMilliseconds);
      
      var invite = inviteMatch.Groups["invite"].Value;
      
      if (config.Invites.Whitelist.Any(inv => inv.VanityURL == invite))
         return Result.FromSuccess();

      if (config.Invites.ScanOrigin)
      {
         var inviteOrigin = await _invites.GetInviteAsync(invite);
         
         if (inviteOrigin.IsSuccess && inviteOrigin.Entity.Guild.IsDefined(out var origin))
            if (config.Invites.Whitelist.Any(inv => inv.GuildId == origin.ID) || origin.ID == guildID)
               return Result.FromSuccess();
      }
      
      //Evaluate if the user is whitelisted.
      var exemptionResult = await _exemptions.EvaluateExemptionAsync(ExemptionCoverage.AntiInvite, guildID, message.Author.ID, message.ChannelID);

      if (!exemptionResult.IsDefined(out var isExempt))
      {
         _logger.LogWarning(EventIds.AutoMod, "Failed to evaluate exemption for {User} in {Guild}", message.Author.ID, guildID);
         return (Result)exemptionResult;
      }
      
      if (isExempt)
         return Result.FromSuccess();
      
      if (config.Invites.DeleteOnMatch)
         await _channels.DeleteMessageAsync(message.ChannelID, message.ID);

      if (config.Invites.WarnOnMatch)
      {
         var selfResult = await _users.GetCurrentUserAsync();
      
         //This should be cached. It should be fine. If it's not it deserves to break.
         if (!selfResult.IsDefined(out var self))
            return (Result)selfResult;

         var infractionResult = await _infractions.StrikeAsync(guildID, message.Author.ID, self.ID, $"Posted a non-whitelisted invite: {invite}");

         if (!infractionResult.IsSuccess)
            _logger.LogWarning(EventIds.AutoMod, "Failed to create infraction for {User} in {Guild} \n{@Error}", message.Author.ID, guildID, infractionResult.Error);
      }
      
      _logger.LogDebug(EventIds.AutoMod, "Invite handling finished in {Time:N0} ms", (DateTimeOffset.UtcNow - message.ID.Timestamp).TotalMilliseconds);
      
      return Result.FromSuccess();
   }
}