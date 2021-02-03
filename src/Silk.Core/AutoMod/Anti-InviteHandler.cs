using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.AutoMod
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
        private static readonly Regex AggressiveRegexPattern = new(@"(discord((app\.com|.com)\/invite|\.gg)\/([A-z]?[0-9]?-?)+)", flags);
        private static readonly Regex LenientRegexPattern = new(@"discord.gg\/invite\/.+", flags);

        private readonly IInfractionService _infractionService;
        private readonly ConfigService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //

        private readonly HashSet<string> _blacklistedLinkCache = new();

        public AutoModInviteHandler(ConfigService configService, IInfractionService infractionService)
        {
            _configService = configService;
            _infractionService = infractionService;
        }


        public Task Invites(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Channel.IsPrivate || eventArgs.Message is null) return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        private async Task CheckForInvite(DiscordClient c, DiscordMessage message, GuildConfig config, string inviteCode)
        {
            if (config.ScanInvites) // If invites should be scanned for their origin
            {
                try
                {
                    DiscordInvite invite = await c.GetInviteByCodeAsync(inviteCode);
                    if (invite.Guild.Id == message.Channel.GuildId) return;

                    Task action = invite.Inviter switch
                    {
                        null when config.AllowedInvites.All(i => i.VanityURL != invite.Code) => // Vanity invite; no inviter
                            AutoModMatchedInviteProcedureAsync(config, message, inviteCode),
                        null when config.AllowedInvites.All(i => i.GuildName != invite.Guild.Name) => // Not a vanity invite, but doesn't point to a recognized guild
                            AutoModMatchedInviteProcedureAsync(config, message, inviteCode),
                        _ when config.AllowedInvites.All(i => i.GuildId != invite.Guild.Id) => // Guild Id doesn't match either
                            AutoModMatchedInviteProcedureAsync(config, message, inviteCode),
                        _ => Task.CompletedTask
                    };

                    await action;
                }
                catch // The invite was either a false-positive, or has garbage behind the actual invite code, making it impossible to match, so it will be deleted as a failsafe.
                {
                    await AutoModMatchedInviteProcedureAsync(config, message, inviteCode);
                }
            }
            else await AutoModMatchedInviteProcedureAsync(config, message, inviteCode); // Don't scan invites; just delete them
        }


        private async Task AutoModMatchedInviteProcedureAsync(GuildConfig config, DiscordMessage message, string invite)
        {
            if (!_blacklistedLinkCache.Contains(invite)) _blacklistedLinkCache.Add(invite);

            bool shouldPunish = await _infractionService.ShouldAddInfractionAsync((DiscordMember) message.Author);
            if (shouldPunish && config.DeleteMessageOnMatchedInvite) _ = message.DeleteAsync();
            if (shouldPunish && config.WarnOnMatchedInvite)
            {
                var infraction = await _infractionService.CreateInfractionAsync((DiscordMember)message.Author,
                    message.Channel.Guild.CurrentMember, InfractionType.Ignore, "Sent an invite");
                
            }
            
        }




    }
}