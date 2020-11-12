using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation
{
    public class BanCommand : BaseCommandModule
    {
        [Command("Ban")]

        public async Task Ban(CommandContext ctx, [HelpDescription("The person to ban")] DiscordMember target, [RemainingText] string reason = "No reason given.")
        {
            var user = await ctx.Guild.GetMemberAsync(target.Id);
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!CanExecuteCommand(out var errorReason))
            {
                await DenyBanAsync(errorReason);
                return;
            }
            async Task DenyBanAsync(string errorReason)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl).WithColor(DiscordColor.Red).WithDescription(errorReason));
            }

            bool CanExecuteCommand(out string errorReason)
            {
                if (target == bot)
                {
                    errorReason = $"I can't ban myself!";
                    return false;
                }
                else if (!ctx.Member.HasPermission(Permissions.BanMembers))
                {
                    errorReason = $"You do not have permission to ban members!";
                    return false;
                }
                else if (user.IsAbove(bot))
                {
                    errorReason = $"{target.Mention} has a role {user.GetHighestRoleMention()} that is above mine, and I cannot ban them!";
                    return false;
                }
                errorReason = null;
                return true;
            }




            var userBannedEmbed = new DiscordEmbedBuilder()
                .WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl)
                .WithTitle($"You've been banned from {ctx.Guild.Name}!")
                .AddField("Reason:", $"{reason}")
                .AddFooter(ctx)
                .WithColor(new DiscordColor("#cc1400"));

            var (name, url) = ctx.GetAuthor();
            var logEmbed = new DiscordEmbedBuilder()
                .WithAuthorExtension(name, url)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($":hammer: {ctx.Member.Mention} banned {target.Mention}!")
                .AddField("Infraction occured:", DateTime.UtcNow.ToString("dd/MM/yy - HH:mm UTC"))
                .AddField("Reason:", reason).AddFooter(ctx);
            try
            {
                await target.SendMessageAsync(embed: userBannedEmbed);
            }
            finally
            {

                await ctx.Guild.BanMemberAsync(user, 7, reason);
                var loggingChannel = SilkBot.Bot.Instance.SilkDBContext.Guilds.Where(guild => guild.Id == ctx.Guild.Id).FirstOrDefault()?.MessageEditChannel;
                var sendChannel = ctx.Guild.GetChannel(loggingChannel.Value) ?? ctx.Channel;

                await sendChannel.SendMessageAsync(embed: logEmbed);
            }





        }
    }
}
