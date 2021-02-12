using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silk.Core.Tools.EventHelpers
{
    class MemberAddedHandler
    {
        private readonly ConfigService _configService;
        private readonly ILogger<MemberAddedHandler> _logger;

        public MemberAddedHandler(ConfigService configService, ILogger<MemberAddedHandler> logger) => (_configService, _logger) = (configService, logger);

        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            var config = await _configService.GetConfigAsync(e.Guild.Id);
            if (config is null) return;

            if (config.LogMemberJoing && config.GeneralLoggingChannel is not 0)
                await (await c.GetChannelAsync(config.GeneralLoggingChannel)).SendMessageAsync(GetJoinEmbed(e, DateTime.Now));

            if (config.GreetMembers
                && config.GreetingChannel is not 0
                && config.GreetingText != "")
                await (await c.GetChannelAsync(config.GreetingChannel)).SendMessageAsync(config.GreetingText.Replace("@u", e.Member.Mention));
        }

        private DiscordEmbedBuilder GetJoinEmbed(GuildMemberAddEventArgs e, DateTime now) => new DiscordEmbedBuilder()
            .WithTitle("User joined:")
            .WithDescription($"User: {e.Member.Mention}")
            .AddField("User ID:", e.Member.Id.ToString(), true)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithFooter("Joined at (UTC)")
            .WithTimestamp(now.ToUniversalTime())
            .WithColor(DiscordColor.Green);
    }
}
