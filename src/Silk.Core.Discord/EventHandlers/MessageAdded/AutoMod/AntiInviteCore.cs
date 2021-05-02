using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod
{
    /// <summary>
    ///     Utility class for all core Auto-Mod features.
    /// </summary>
    public class AntiInviteCore
    {
        private static ILogger<AntiInviteCore> _logger;
        public AntiInviteCore(ILogger<AntiInviteCore> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Regex to match discord invites using discord's main invite URL (discord.gg)
        /// </summary>
        public static Regex LenientRegexPattern { get; } = new(@"discord.gg\/([A-z]*-*[0-9]*){2,}", FlagConstants.RegexFlags);

        /// <summary>
        ///     A more aggressive regex to match anything that could be considered an invite/attempt to circumvent <see cref="LenientRegexPattern" />.
        ///     Includes, but is not limited to discord.gg, discord.com/invite, and disc.gg
        /// </summary>
        public static Regex AggressiveRegexPattern { get; } = new(@"disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-]{2,})", FlagConstants.RegexFlags);

        /// <summary>
        ///     Checks if a <see cref="DiscordMessage" /> has a valid <see cref="DiscordInvite" />.
        /// </summary>
        /// <param name="client">Client instance used to make API calls as necessary.</param>
        /// <param name="message">The message to check.</param>
        /// <param name="config">The configuration of the guild the message was sent on.</param>
        /// <returns>Whether further action should be taken</returns>
        public static async Task<bool> CheckForInviteAsync(DiscordClient client, DiscordMessage message, GuildConfig config)
        {
            if (!config.BlacklistInvites) return false;
            if (message.Channel.IsPrivate) return false;
            if (message.Author.IsBot) return false;

            Regex scanPattern = config.UseAggressiveRegex ? AggressiveRegexPattern : LenientRegexPattern;
            Match match = scanPattern.Match(message.Content);

            if (match.Success)
            {
                string code = match.Groups.Values.Last().Captures.First().Value;
                return await IsBlacklistedInvite(client, message, config, code);
            }
            return false;
        }

        /// <summary>
        ///     Checks if a suspected <see cref="DiscordInvite" /> is blacklisted.
        /// </summary>
        /// <param name="client">A client object to make API calls with.</param>
        /// <param name="message">The message to check.</param>
        /// <param name="config">The guild configuration, to determine whether an API call should be made.</param>
        /// <param name="invite">The invite to check.</param>
        /// <returns>Whether Auto-Mod should progress with the infraction steps regarding invites.</returns>
        public static async Task<bool> IsBlacklistedInvite(DiscordClient client, DiscordMessage message, GuildConfig config, string invite)
        {


            var handleInvite = true;
            if (config.ScanInvites)
            {
                try
                {
                    DiscordInvite apiInvite = await client.GetInviteByCodeAsync(invite);

                    if (apiInvite.Guild.Id != message.Channel.GuildId)
                    {
                        handleInvite = config.AllowedInvites.All(inv => apiInvite.Guild.Id != inv.GuildId);
                    }
                    else
                    {
                        handleInvite = false;
                        _logger.LogTrace("Matched invite points to current guild; skipping");
                    }
                }
                catch (NotFoundException) // Discord throws 404 if you ask for an invalid invite. i.e. Garbage behind a legit code. //
                {
                    _logger.LogTrace("Matched invalid or corrupt invite");
                }
            }
            return handleInvite;
        }

        /// <summary>
        ///     Checks if a memeber should be punished for sending a Discord invite, and calls <see cref="IInfractionService.ProgressInfractionStepAsync" /> if
        ///     it succeeds.
        /// </summary>
        /// <param name="config">The configuration for the guild.</param>
        /// <param name="message">The offending message, to be deleted, if configured.</param>
        /// <param name="infractionService">The infraction service reference to make a call to if checks succeed.</param>
        public static async Task TryAddInviteInfractionAsync(GuildConfig config, DiscordMessage message, IInfractionService infractionService)
        {
            bool shouldPunish = await infractionService.ShouldAddInfractionAsync((DiscordMember) message.Author);
            if (shouldPunish && config.DeleteMessageOnMatchedInvite) _ = message.DeleteAsync();
            if (shouldPunish && config.WarnOnMatchedInvite)
            {
                var infraction = await infractionService.CreateInfractionAsync((DiscordMember) message.Author,
                    message.Channel.Guild.CurrentMember, InfractionType.Warn, "[AUTO-MOD] Sending an invite link");
                await infractionService.ProgressInfractionStepAsync((DiscordMember) message.Author, "");
            }
        }
    }
}