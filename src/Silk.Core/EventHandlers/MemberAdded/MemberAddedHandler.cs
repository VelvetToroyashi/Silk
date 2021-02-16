using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.EventHandlers.MemberAdded
{
    public class MemberAddedHandler
    {
        private readonly ConfigService _configService;
        public MemberAddedHandler(ConfigService configService)
        {
            _configService = configService;
        }
        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            GuildConfig config = await _configService.GetConfigAsync(e.Guild.Id);

            if (config.LogMemberJoing && config.GeneralLoggingChannel is not 0)
            {
                await (await c.GetChannelAsync(config.GeneralLoggingChannel)).SendMessageAsync(GetJoinEmbed(e, DateTime.UtcNow));
            }

            if (config.GreetMembers && config.GreetingChannel is not 0 && !string.IsNullOrWhiteSpace(config.GreetingText))
            {
                DiscordChannel channel = await c.GetChannelAsync(config.GreetingChannel);
                string formattedMessage = config.GreetingText
                    .Replace("{u}", e.Member.Username)
                    .Replace("{s}", e.Guild.Name)
                    .Replace("{@u}", e.Member.Mention);

                await channel.SendMessageAsync(formattedMessage);
            }
        }

        private static DiscordEmbedBuilder GetJoinEmbed(GuildMemberAddEventArgs e, DateTime now) => new DiscordEmbedBuilder()
            .WithTitle("User joined:")
            .WithDescription($"User: {e.Member.Mention}")
            .AddField("User ID:", e.Member.Id.ToString(), true)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithColor(DiscordColor.Green);
    }
}