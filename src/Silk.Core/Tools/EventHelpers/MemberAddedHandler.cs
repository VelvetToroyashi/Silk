using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Services;
using System;
using System.Threading.Tasks;
using Silk.Core.Database.Models;

namespace Silk.Core.Tools.EventHelpers
{
    class MemberAddedHandler
    {
        private readonly ConfigService _configService;
        private readonly ILogger<MemberAddedHandler> _logger;

        public MemberAddedHandler(ConfigService configService, ILogger<MemberAddedHandler> logger) => (_configService, _logger) = (configService, logger);

        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            GuildConfig config = await _configService.GetConfigAsync(e.Guild.Id);

            if (config.LogMemberJoing && config.GeneralLoggingChannel is not 0)
            {
                DiscordChannel channel = await c.GetChannelAsync(config.GeneralLoggingChannel);
                await channel.SendMessageAsync(GetJoinEmbed(e, DateTime.UtcNow));
            }   

            if (config.GreetMembers && config.GreetingChannel is not 0 && !string.IsNullOrWhiteSpace(config.GreetingText))
            {
                DiscordChannel channel = await c.GetChannelAsync(config.GreetingChannel);
                await channel.SendMessageAsync(config.GreetingText.Replace("@u", e.Member.Mention));
            }
        }

        private static DiscordEmbedBuilder GetJoinEmbed(GuildMemberAddEventArgs e, DateTime now) => new DiscordEmbedBuilder()
            .WithTitle("User joined:")
            .WithDescription($"User: {e.Member.Mention}")
            .AddField("User ID:", e.Member.Id.ToString(), true)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithFooter("Joined at (UTC)")
            .WithTimestamp(now)
            .WithColor(DiscordColor.Green);
    }
}
