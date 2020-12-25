using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.AutoMod
{
    public class AutoModMessageHandler
    {
        private static readonly Regex AGGRESSIVE_REGEX = new(@"discord((app\.com|\.com)\/invite|\.gg\/.+)");
        private static readonly Regex LENIENT_REGEX    = new(@"discord.gg\/invite\/.+");
        
        private readonly DatabaseService _dbService; // Used to querying user from the DB, though I may switch this out //
                                                    // Perhaps something along the lines of rewriting that infraction service ~Velvet //
        private readonly GuildConfigCacheService _configService; // Pretty self-explanatory; used for caching the guild configs to make sure they've enabled AutoMod //
        private readonly ILogger<AutoModMessageHandler> _logger; // Log infractions, or soemthing. //
        
        // I despise of having the ctor broken up like this, but it's w/e; my fault for making the class name so long //
        public AutoModMessageHandler
            (DatabaseService dbService, GuildConfigCacheService configService, ILogger<AutoModMessageHandler> logger) => 
            (_dbService, _configService, _logger) = (dbService, configService, logger);

        public async Task CheckForInvites(DiscordClient c, MessageCreateEventArgs e)
        {
            
            _ = Task.Run(async () =>
            {
                GuildConfigModel guild = await _configService.GetConfigAsync(e.Guild.Id);
                if (!guild.BlacklistInvites) return;
                Regex matchingPattern = guild.UseAggressiveRegex ? AGGRESSIVE_REGEX : LENIENT_REGEX;

                if (matchingPattern.IsMatch(e.Message.Content))
                {
                    await e.Channel.SendMessageAsync("No invites smh.");
                }

            });
        }
        



    }
}