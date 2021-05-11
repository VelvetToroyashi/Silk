using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services;

namespace Silk.Core.Discord.EventHandlers.Reactions
{
    public class ReactionAddedHandlerService
    {
        private readonly ConfigService _configCache;
        private readonly ILogger<ReactionAddedHandlerService> _logger;
        public ReactionAddedHandlerService(ConfigService configCache, ILogger<ReactionAddedHandlerService> logger)
        {
            _configCache = configCache;
            _logger = logger;
        }

        public async Task Handle(DiscordClient _, MessageReactionAddEventArgs args)
        {
            if (args.Channel.IsPrivate) return;
            ulong msgId = args.Message.Id;

            GuildConfig config = await _configCache.GetConfigAsync(args.Guild.Id); // Pulls from cache //
            _logger.LogTrace("Grabbed config");
            RoleMenu? value = config.RoleMenus.SingleOrDefault(r => r.MessageId == msgId);
            if (value is not null) _logger.LogTrace("Valid role menu!");
            if (value is null) return;

            if (value.RoleDictionary.TryGetValue(args.Emoji.Name, out ulong id))
            {
                _logger.LogTrace("Role exists on server!");
                DiscordRole role = args.Guild.GetRole(id);
                await (args.User as DiscordMember)!.GrantRoleAsync(role);
                _logger.LogTrace("Gave {User} @{RoleName}", args.User.Username, role.Name);
            }
        }
    }
}