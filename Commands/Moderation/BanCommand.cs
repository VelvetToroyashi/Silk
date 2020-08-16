using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;
using System;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation
{
    public class BanCommand : BaseCommandModule
    {
        [Command("Ban")]
        [HelpDescription("Ban someone! Both Silk and Invoker require `Ban Members`.", "<prefix>ban <userID>/<mention>")]
        public async Task Ban(CommandContext ctx, [HelpDescription("The person to ban")] DiscordUser target, [RemainingText] string reason = "Not given.")
        {
            var user = await ctx.Guild.GetMemberAsync(target.Id);
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if(!CanExecuteCommand(out reason))
            {
                await DenyBanAsync(reason);
                return;
            }

            async Task DenyBanAsync(string reason)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Red).WithDescription(reason)
                                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                                .WithTimestamp(DateTime.Now));
            }

            bool CanExecuteCommand(out string reason)
            {
                if (target == bot)
                {
                    reason = $"I can't ban myself!";
                    return false;
                }
                if (!ctx.Member.HasPermission(Permissions.BanMembers))
                {
                    reason = $"You do not have permission to ban members!";
                    return false;
                }
                if (user.IsAbove(bot))
                {
                    reason = $"{target.Mention} has a role {user.GetHighestRoleMention()} that is above mine, and I cannot ban them!";
                    return false;
                }
                    reason = null;
                    return true;
            }




            var userBannedEmbed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, $"You've been banned from {ctx.Guild.Name}!", "")).AddField("Reason:", $"{(reason == "" ? "No reason provided." : reason)}");
            try
            {
                await DMCommand.DM(ctx, target, userBannedEmbed);
            }
            catch (Exception e) { ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "Silk", e.Message, DateTime.Now, e); }



            await ctx.Guild.BanMemberAsync(user, 7, reason == "" ? "No reason provided" : reason);

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($":hammer: banned {target.Mention}!")
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));

        }
    }
}
