using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;

namespace Silk.Core.EventHandlers.Messages.AutoMod;

public class MessageEditAntiInvite
{
    private readonly GuildConfigCacheService    _guildConfigCacheService;
    private readonly AntiInviteHelper _inviteHelper;
    public MessageEditAntiInvite(GuildConfigCacheService guildConfigCacheService, AntiInviteHelper inviteHelper)
    {
        _guildConfigCacheService = guildConfigCacheService;
        _inviteHelper = inviteHelper;
    }

    public async Task CheckForInvite(DiscordClient client, MessageUpdateEventArgs args)
    {
        if (args.Channel.IsPrivate) return;
        GuildModConfigEntity? config = await _guildConfigCacheService.GetModConfigAsync(args.Guild.Id);
        bool hasInvite = _inviteHelper.CheckForInvite(args.Message, config, out string invite);
        bool isBlacklisted = await _inviteHelper.IsBlacklistedInvite(args.Message, config, invite);

        if (hasInvite && isBlacklisted)
            await _inviteHelper.TryAddInviteInfractionAsync(args.Message, config);
    }
}