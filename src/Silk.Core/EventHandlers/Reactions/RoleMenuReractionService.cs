using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Services;

namespace Silk.Core.EventHandlers.Reactions
{
    public class RoleMenuReractionService
    {
        private readonly ConfigService _configCache;
        private readonly ILogger<RoleMenuReractionService> _logger;
        public RoleMenuReractionService(ConfigService configCache, ILogger<RoleMenuReractionService> logger)
        {
            _configCache = configCache;
            _logger = logger;
        }

        public async Task Add(DiscordClient _, MessageReactionAddEventArgs args)
        {
            if (args.User.IsBot || args.Channel.IsPrivate) return; // ??? //

            ulong msgId = args.Message.Id;

            GuildConfig config = await _configCache.GetConfigAsync(args.Guild.Id); // Pulls from cache //
            RoleMenu? value = config.RoleMenus.SingleOrDefault(r => r.MessageId == msgId);
            if (value is null) return;

            if (value.RoleDictionary.TryGetValue(args.Emoji.Name, out ulong id))
            {
                DiscordRole role = args.Guild.GetRole(id);
                await (args.User as DiscordMember)!.GrantRoleAsync(role);
            }
        }

        public async Task Remove(DiscordClient _, MessageReactionRemoveEventArgs args)
        {
            if (args.User.IsBot || args.Channel.IsPrivate) return; // ??? //

            ulong msgId = args.Message.Id;
            GuildConfig config = await _configCache.GetConfigAsync(args.Guild.Id); // Pulls from cache //
            RoleMenu? value = config.RoleMenus.SingleOrDefault(r => r.MessageId == msgId);

            if (value is null) return;

            if (value.RoleDictionary.TryGetValue(args.Emoji.Name, out ulong id))
            {
                DiscordRole role = args.Guild.GetRole(id);
                await (args.User as DiscordMember)!.RevokeRoleAsync(role);
            }
        }
    }
}