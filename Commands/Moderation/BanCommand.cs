using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation
{
    public class BanCommand : BaseCommandModule
    {
        [Command("ban")]
        public async Task Ban(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);


            var banDeniedReason = "";
            if (user == bot)
                banDeniedReason = "I can't kick myself!";
            else if (user == ctx.Guild.Owner)
                banDeniedReason = $"I can't ban {user.Mention}...They're the owner...";

            if (user.Roles.Any())
            {
                if (user.Roles.Last().Permissions.HasPermission(Permissions.KickMembers))
                    banDeniedReason = $"I can't ban {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})";
                else if (user.Roles.Last().Permissions.HasPermission(Permissions.BanMembers))
                    banDeniedReason = $"I can't ban {user.Mention}! They're an admin! ({user.Roles.Last().Mention})";
            }


            if (banDeniedReason != "")
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                                .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Red)
                                                .WithDescription(banDeniedReason)
                                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                                .WithTimestamp(DateTime.Now));
                return;
            }



            var embed = new DiscordEmbedBuilder(EmbedGenerator.CreateEmbed(ctx, $"You've been banned from {ctx.Guild.Name}!", "")).AddField("Reason:", $"{(reason == "" ? "No reason provided." : reason)}");
            try
            {
                await DMCommand.DM(ctx, user, embed);
            }
            catch (Exception e) { }



            await ctx.Guild.BanMemberAsync(user, 7, reason == "" ? "No reason provided" : reason);

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($":hammer: banned {user.Mention}!")
                .WithFooter("Silk")
                .WithTimestamp(DateTime.Now));

        }
    }
}
