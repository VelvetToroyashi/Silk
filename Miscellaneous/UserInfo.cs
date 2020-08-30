using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Miscellaneous
{
    public class UserInfo : BaseCommandModule
    {
        [Command("userinfo")]
        [Aliases("info")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(member.DisplayName, iconUrl: member.AvatarUrl)
                .WithDescription($"Information I could pull on {member.Mention}!")
                .WithColor(DiscordColor.Orange)
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            var status = "";
            DiscordEmoji emoji = null;
            switch (member.Presence.Status)
            {
                case UserStatus.Online:
                    status = "Online";
                    emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339430672203796);
                    break;
                case UserStatus.Idle:
                    status = "Idle";
                    emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431720910889);
                    break;
                case UserStatus.DoNotDisturb:
                    status = "Do Not Disturb";
                    emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431632568450);
                    break;
                case UserStatus.Offline:
                    status = "Offline";
                    emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431905198100);
                    break;
            }

            embed.AddField("Status:", $"{emoji}{status}"); 
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", member.CreationTimestamp.ToString());
            var roleList = new List<string>();
            foreach (var role in member.Roles.OrderByDescending(r => r.Position))
                roleList.Add(role.Mention);
            embed.AddField("Roles:", string.Join(null, roleList));

            await ctx.RespondAsync(embed: embed);
        }
    }
}
