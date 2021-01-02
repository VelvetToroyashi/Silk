using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.AutoMod
{
    public class AutoModMessageHandler
    {
        private static readonly RegexOptions flags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;
        private static readonly Regex AgressiveRegexPattern = new(@"(discord((app\.com|.com)\/invite|\.gg)\/[A-z]+)", flags);
        private static readonly Regex LenientRegexPattern    = new(@"discord.gg\/invite\/.+", flags);

        private readonly InfractionService _infractionService; // I'll implement this soon. //
        private readonly ConfigService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //

        private readonly HashSet<string> _blacklistedLinkCache = new();
        
        
        public AutoModMessageHandler(ConfigService configService, InfractionService infractionService) => 
             (_configService, _infractionService) = (configService, infractionService);


        public Task CheckForInvites(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate) return Task.CompletedTask;
            _ = Task.Run(async () => //Before you ask: Task.Run() in event handlers because await = block
            {
                GuildConfigModel config = await _configService.GetConfigAsync(e.Guild.Id);
                if (!config.BlacklistInvites) return;
                Regex matchingPattern = config.UseAggressiveRegex ? AgressiveRegexPattern : LenientRegexPattern;
                Match match = matchingPattern.Match(e.Message.Content);
                if (match.Success)
                {
                    int codeStart = match.Value.LastIndexOf('/');
                    string code = match.Value[codeStart..];
                    if (config.ScanInvites)
                    {
                        DiscordInvite invite = await c.GetInviteByCodeAsync(code);
                        // Vanity invite and no vanity URL matches //
                        if (invite.Inviter is null && config.AllowedInvites.All(i => i.VanityURL != invite.Code))
                            await AutoModMatchedInvitePrecedureAsync(config, e.Message);

                        if (config.AllowedInvites.All(inv => inv.GuildName != invite.Guild.Name))
                            await AutoModMatchedInvitePrecedureAsync(config, e.Message);
                    }
                    else await AutoModMatchedInvitePrecedureAsync(config, e.Message); // I can't think of a better name. //
                }
            });
            return Task.CompletedTask;
        }
        private async Task AutoModMatchedInvitePrecedureAsync(GuildConfigModel config, DiscordMessage message)
        {
            
            if (config.DeleteMessageOnMatchedInvite) await message.DeleteAsync();
            if (config.WarnOnMatchedInvite) return; // Coming Soon™️ //
        }
        



    }
}