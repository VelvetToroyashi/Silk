using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Silk.Core.Database.Models;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.AutoMod
{
    public class MessageAddedHandler
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

        private readonly IInfractionService _infractionService;
        private readonly ConfigService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //

        private readonly HashSet<string> _blacklistedLinkCache = new();
        
        public MessageAddedHandler(ConfigService configService, IInfractionService infractionService) => (_configService, _infractionService) = (configService, infractionService);


        public Task Invites(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate || e.Message is null) return Task.CompletedTask;
            _ = Task.Run(async () => 
            {
                GuildConfigModel config = await _configService.GetConfigAsync(e.Guild.Id);
                if (!config.BlacklistInvites) return;
                
                Regex matchingPattern = config.UseAggressiveRegex ? AgressiveRegexPattern : LenientRegexPattern;
                
                Match match = matchingPattern.Match(e.Message.Content);
                if (match.Success)
                {
                    int codeStart = match.Value.LastIndexOf('/');
                    string code = match.Value[(codeStart+1)..];
                    
                    if (_blacklistedLinkCache.Contains(code))
                        AutoModMatchedInvitePrecedureAsync(config, e.Message, code).GetAwaiter();
                    else CheckForInvite(c, e.Message, config, code);
                }
            });
            return Task.CompletedTask;
        }

        private void CheckForInvite(DiscordClient c,  DiscordMessage message, GuildConfigModel config, string inviteCode)
        {
            if (config.ScanInvites)
            {
                DiscordInvite invite = c.GetInviteByCodeAsync(inviteCode).GetAwaiter().GetResult();
                if (invite.Inviter is null && config.AllowedInvites.All(i => i.VanityURL != invite.Code))
                    AutoModMatchedInvitePrecedureAsync(config, message, inviteCode).GetAwaiter();
                else if (invite.Inviter is null && config.AllowedInvites.All(i => i.VanityURL != invite.Code))
                    AutoModMatchedInvitePrecedureAsync(config, message, inviteCode).GetAwaiter();
            }
            else AutoModMatchedInvitePrecedureAsync(config, message, inviteCode).GetAwaiter();
        }
        
        
        private async Task AutoModMatchedInvitePrecedureAsync(GuildConfigModel config, DiscordMessage message, string invite)
        {
            if (!_blacklistedLinkCache.Contains(invite)) _blacklistedLinkCache.Add(invite);
            if (await _infractionService.ShouldDeleteMessageAsync((DiscordMember) message.Author)) await message.DeleteAsync();
            else return;
            _infractionService.AddInfraction((DiscordMember)message.Author, new());
            // Coming Soon™️ //
        }
        



    }
}