using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.AutoMod
{
    public class AutoModMessageHandler
    {
        private static readonly RegexOptions flags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        /*
         * To those unacquinted to Regex, or simply too lazy to plug it into regex101.com,
         * these two Regexes match Discord invites. The reason we don't simply do something like Message.Contains("discord.gg/") || Message.Contains("discord.com/inv..
         * is because that's not only bulky, but its also ugly, and *possibly* slighty slower thanks to extra if-statements. Granted, still probably blazing fast, but
         * I can't be asked to implement that abomination of a pattern when we can just use a regex, and conveniently get what we want out of it without any extra work.
         *
         * And again, for the curious ones, the former regex will match anything that resembles an invite.
         * For instance, discord.gg/HZfZb95, discord.com/invite/HZfZb95, discordapp.com/invite/HZfZb95
         */
        private static readonly Regex AgressiveRegexPattern = new(@"(discord((app\.com|.com)\/invite|\.gg)\/[A-z]+)", flags);
        private static readonly Regex LenientRegexPattern    = new(@"discord.gg\/invite\/.+", flags);

        private readonly InfractionService _infractionService; // I'll implement this soon. //
        private readonly ConfigService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //

        private readonly HashSet<string> _blacklistedLinkCache = new();
        
        public AutoModMessageHandler(ConfigService configService, InfractionService infractionService) => (_configService, _infractionService) = (configService, infractionService);


        public Task CheckForInvites(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate) return Task.CompletedTask;
            _ = Task.Run(async () => //Before you ask: Task.Run() in event handlers because await = block
            {
                Stopwatch sw = Stopwatch.StartNew();
                GuildConfigModel config = await _configService.GetConfigAsync(e.Guild.Id);
                Log.Logger.Verbose($"Grabbed configuration in {sw.ElapsedMilliseconds} ms!");
                sw.Restart();
                if (!config.BlacklistInvites) return;
                Regex matchingPattern = config.UseAggressiveRegex ? AgressiveRegexPattern : LenientRegexPattern;
                Match match = matchingPattern.Match(e.Message.Content);
                if (match.Success)
                {
                    int codeStart = match.Value.LastIndexOf('/');
                    string code = match.Value[(codeStart+1)..];
                    if (_blacklistedLinkCache.Contains(code))
                    {
                        AutoModMatchedInvitePrecedureAsync(config, e.Message, code).GetAwaiter();
                        Log.Logger.Verbose($"Caught and deleted invite in {sw.ElapsedMilliseconds} ms!");
                        return;
                    }
                    if (config.ScanInvites)
                    {
                        DiscordInvite invite = await c.GetInviteByCodeAsync(code);
                        // Vanity invite and no vanity URL matches //
                        if (invite.Inviter is null && config.AllowedInvites.All(i => i.VanityURL != invite.Code))
                            AutoModMatchedInvitePrecedureAsync(config, e.Message, code).GetAwaiter();

                        if (config.AllowedInvites.All(inv => inv.GuildName != invite.Guild.Name))
                             AutoModMatchedInvitePrecedureAsync(config, e.Message, code).GetAwaiter();
                    }
                    else AutoModMatchedInvitePrecedureAsync(config, e.Message, code).GetAwaiter(); // I can't think of a better name. //
                }
            });
            return Task.CompletedTask;
        }
        private async Task AutoModMatchedInvitePrecedureAsync(GuildConfigModel config, DiscordMessage message, string invite)
        {
            if (!_blacklistedLinkCache.Contains(invite)) _blacklistedLinkCache.Add(invite);
            if (config.DeleteMessageOnMatchedInvite) await message.DeleteAsync();
            if (config.WarnOnMatchedInvite) return; // Coming Soon™️ //
        }
        



    }
}