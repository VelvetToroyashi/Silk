using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Core.EventHandlers.MemberRemoved
{
    public class MemberRemovedHandler
    {
        private readonly ConfigService _configService;

        public MemberRemovedHandler(ConfigService configService, ILogger<MemberRemovedHandler> logger)
        {
            _configService = configService;
        }

        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            GuildConfig config = await _configService.GetConfigAsync(e.Guild.Id);
            // This should be done in a seperate service //
            if (config.LogMemberLeaves && config.LoggingChannel is not 0)
                await e.Guild.GetChannel(config.LoggingChannel).SendMessageAsync(GetLeaveEmbed(e));
        }

        private static DiscordEmbedBuilder GetLeaveEmbed(GuildMemberAddEventArgs e)
        {
            return new DiscordEmbedBuilder()
                .WithTitle("User joined:")
                .WithDescription($"User: {e.Member.Mention}")
                .AddField("User ID:", e.Member.Id.ToString(), true)
                .WithThumbnail(e.Member.AvatarUrl)
                .WithColor(DiscordColor.Green);
        }
    }
}
