using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Types;
using Silk.Core.Utilities;

namespace Silk.Core.EventHandlers.MemberAdded
{
    public sealed class MemberGreetingService
    {
        private readonly ConfigService _configService;

        private readonly AsyncTimer _timer;
        public MemberGreetingService(ConfigService configService, ILogger<MemberGreetingService> logger)
        {
            _configService = configService;
            _timer = new(OnTick, TimeSpan.FromSeconds(1));
            _timer.Start();
        }
        public List<DiscordMember> MemberQueue { get; } = new();

        public async Task OnMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            GuildConfig? config = await _configService.GetConfigAsync(e.Guild.Id);
            if (config is null!) // Wasn't cached yet //
                return;
            // This should be done in a seperate service //
            if (config.LogMemberJoins && config.LoggingChannel is not 0)
                await e.Guild.GetChannel(config.LoggingChannel).SendMessageAsync(GetJoinEmbed(e));

            bool screenMembers = e.Guild.Features.Contains("MEMBER_VERIFICATION_GATE_ENABLED") && config.GreetingOption is GreetingOption.GreetOnScreening;
            bool verifyMembers = config.GreetingOption is GreetingOption.GreetOnRole && config.VerificationRole is not 0;

            if (screenMembers || verifyMembers)
                MemberQueue.Add(e.Member);
            else await GreetMemberAsync(e.Member, config);
        }

        private static async Task GreetMemberAsync(DiscordMember member, GuildConfig config)
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

        private void OnTick(object _, ElapsedEventArgs __) => AsyncUtil.RunSync(async () => await OnTick());
        private async Task OnTick()
        {
            if (MemberQueue.Count is 0) 
                return;
            
            foreach (DiscordMember member in MemberQueue)
            {
                GuildConfig config = (await _configService.GetConfigAsync(member.Guild.Id))!;

            if (config.GreetingOption is GreetingOption.GreetOnJoin)
            {
                await GreetMemberAsync(member, config);
                MemberQueue.Remove(member);
                continue;
            }

            if (config.GreetingOption is GreetingOption.GreetOnScreening && member.IsPending is true) 
                continue;

            if (config.GreetingOption is GreetingOption.GreetOnRole && !member.Roles.Select(r => r.Id).Contains(config.VerificationRole)) 
                continue;
            await GreetMemberAsync(member, config);
            MemberQueue.Remove(member);
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