using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;
using Silk.Core.Types;

namespace Silk.Core.EventHandlers.MemberAdded
{
    public sealed class MemberGreetingService
    {
        private readonly ConfigService _configService;

        private readonly AsyncTimer _timer;
        public MemberGreetingService(DiscordClient client, ConfigService configService, ILogger<MemberGreetingService> logger)
        {
            client.GuildMemberAdded += OnMemberAdded;

            _configService = configService;
            _timer = new(OnTick, TimeSpan.FromSeconds(1));
            _timer.Start();
        }
        public List<DiscordMember> MemberQueue { get; } = new();

        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            GuildConfigEntity? config = await _configService.GetConfigAsync(e.Guild.Id);
            GuildModConfigEntity? modConfig = await _configService.GetModConfigAsync(e.Guild.Id);

            if (config is null) // Wasn't cached yet //
                return;

            // This should be done in a separate service //
            if (modConfig.LogMemberJoins && modConfig.LoggingChannel is not 0)
                await e.Guild.GetChannel(modConfig.LoggingChannel).SendMessageAsync(GetJoinEmbed(e));

            bool screenMembers = e.Guild.Features.Contains("MEMBER_VERIFICATION_GATE_ENABLED") && config.GreetingOption is GreetingOption.GreetOnScreening;
            bool verifyMembers = config.GreetingOption is GreetingOption.GreetOnRole           && config.VerificationRole is not 0;

            if (screenMembers || verifyMembers)
                MemberQueue.Add(e.Member);
            else await GreetMemberAsync(e.Member, config);
        }

        private static async Task GreetMemberAsync(DiscordMember member, GuildConfigEntity config)
        {
            bool shouldGreet = config.GreetingOption is not GreetingOption.DoNotGreet;
            bool hasValidGreetingChannel = config.GreetingChannel is not 0;
            bool hasValidGreetingMessage = !string.IsNullOrWhiteSpace(config.GreetingText);
            if (shouldGreet && hasValidGreetingChannel && hasValidGreetingMessage)
            {
                DiscordChannel channel = member.Guild.GetChannel(config.GreetingChannel);
                string formattedMessage = config.GreetingText
                                                .Replace("{u}", member.Username)
                                                .Replace("{s}", member.Guild.Name)
                                                .Replace("{@u}", member.Mention)
                                                .Replace("\\n", "\n");

                await channel.SendMessageAsync(formattedMessage);
            }
        }

        private async Task OnTick()
        {
            if (MemberQueue.Count is 0)
                return;

            for (var i = 0; i < MemberQueue.Count; i++)
            {
                DiscordMember member = MemberQueue[i];
                GuildConfigEntity config = (await _configService.GetConfigAsync(member.Guild.Id));

                if (config.GreetingOption is GreetingOption.GreetOnScreening && member.IsPending is true)
                    continue;

                if (config.GreetingOption is GreetingOption.GreetOnRole && !member.Roles.Select(r => r.Id).Contains(config.VerificationRole))
                    continue;

                MemberQueue.Remove(member);
                await GreetMemberAsync(member, config);
            }
        }

        private static DiscordEmbedBuilder GetJoinEmbed(GuildMemberAddEventArgs e)
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