using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.EventHandlers.MessageAdded.AutoMod
{
    public class AutoModInviteHandler
    {
        private const RegexOptions flags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        /*
         * To those unacquainted to Regex, or simply too lazy to plug it into regex101.com,
         * these two Regexes match Discord invites. The reason we don't simply do something like Message.Contains("discord.gg/") || Message.Contains("discord.com/inv..
         * is because that's not only bulky, but its also ugly, and *possibly* slightly slower thanks to extra if-statements. Granted, still probably blazing fast, but
         * I can't be asked to implement that abomination of a pattern when we can just use a regex, and conveniently get what we want out of it without any extra work.
         *
         * And again, for the curious ones, the former regex will match anything that resembles an invite.
         * For instance, discord.gg/HZfZb95, discord.com/invite/HZfZb95, discordapp.com/invite/HZfZb95
         */
        private static readonly Regex AggressiveRegexPattern = new(@"(discord((app\.com|.com)\/invite|\.gg)\/([A-z]|[0-9]-?)+)", flags);
        private static readonly Regex LenientRegexPattern = new(@"discord.gg\/invite\/.+", flags);

        private readonly IInfractionService _infractionService;
        private readonly ConfigService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //

        private readonly HashSet<string> _blacklistedLinkCache = new();
        private readonly ILogger<AutoModInviteHandler> _logger;

        public AutoModInviteHandler(ConfigService configService, IInfractionService infractionService, ILogger<AutoModInviteHandler> logger)
        {
            _configService = configService;
            _infractionService = infractionService;
            _logger = logger;
        }

        private record UnifiedEventArgs(DiscordChannel Channel, DiscordMessage Message, DiscordGuild Guild);


        // Can't be DRY compliant here because they take two different types of event args, hence why we make one unified object, and call that method instead.
        public Task MessageEditInvites(DiscordClient client, MessageUpdateEventArgs eventArgs)
        {

            if (eventArgs.Channel.IsPrivate) return Task.CompletedTask;
            _ = MatchInvite(client, new(eventArgs.Channel, eventArgs.Message, eventArgs.Guild));
            return Task.CompletedTask;
        }

        public Task MessageAddInvites(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Channel.IsPrivate) return Task.CompletedTask;
            _ = MatchInvite(client, new(eventArgs.Channel, eventArgs.Message, eventArgs.Guild));
            return Task.CompletedTask;
        }

        private async Task MatchInvite(DiscordClient client, UnifiedEventArgs eventArgs)
        {
            if (eventArgs.Channel.IsPrivate) return;

            _ = Task.Run(async () =>
            {
                GuildConfig config = await _configService.GetConfigAsync(eventArgs.Guild.Id);
                if (!config.BlacklistInvites) return;

                Regex matchingPattern = config.UseAggressiveRegex ? AggressiveRegexPattern : LenientRegexPattern;

                Match match = matchingPattern.Match(eventArgs.Message.Content);
                if (match.Success)
                {
                    int codeStart = match.Value.LastIndexOf('/') + 1;
                    string code = match.Value[codeStart..];

                    if (_blacklistedLinkCache.Contains(code))
                        AutoModMatchedInviteProcedureAsync(config, eventArgs.Message, code).GetAwaiter();
                    else await CheckForInvite(client, eventArgs.Message, config, code);
                }
            });
        }


        /// <summary>
        /// Automod method called when the guild configred regex (be it 'lenient' or 'aggressive' matches anything that resembles an invite.
        /// </summary>
        private async Task CheckForInvite(DiscordClient client, DiscordMessage message, GuildConfig config, string inviteCode)
        {
            var handleInvite = true;
            if (config.ScanInvites)
            {
                try
                {
                    DiscordInvite apiInvite = await client.GetInviteByCodeAsync(inviteCode);

                    if (apiInvite.Guild.Id != message.Channel.GuildId)
                    {
                        handleInvite = !config.AllowedInvites.Any(invite => apiInvite.Code == invite.VanityURL ||
                                                                            apiInvite.Guild.Id == invite.GuildId);
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

            if (handleInvite) await AutoModMatchedInviteProcedureAsync(config, message, inviteCode);
        }

        /// <summary>
        /// Method responsible for determining what the appropriate action is to take, if any depending on
        /// how the guild is configured.
        /// </summary>
        private async Task AutoModMatchedInviteProcedureAsync(GuildConfig config, DiscordMessage message, string invite)
        {
            if (!_blacklistedLinkCache.Contains(invite)) _blacklistedLinkCache.Add(invite);

            bool shouldPunish = await _infractionService.ShouldAddInfractionAsync((DiscordMember) message.Author);
            if (shouldPunish && config.DeleteMessageOnMatchedInvite) _ = message.DeleteAsync();
            if (shouldPunish && config.WarnOnMatchedInvite)
            {
                var infraction = await _infractionService.CreateInfractionAsync((DiscordMember) message.Author,
                    message.Channel.Guild.CurrentMember, InfractionType.Ignore, "Sent an invite");
                await _infractionService.ProgressInfractionStepAsync((DiscordMember) message.Author, infraction);
            }
        }
    }
}