using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services;

namespace Silk.Core.Discord.EventHandlers.Reactions
{
    public class ReactionAddedHandlerService
    {
        private readonly ConfigService _configCache;
        public ReactionAddedHandlerService(ConfigService configCache) => _configCache = configCache;

        public async Task Handle(MessageReactionAddEventArgs args)
        {
            if (args.Channel.IsPrivate) return;
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
    }
}