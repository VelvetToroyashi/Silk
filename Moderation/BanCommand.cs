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
        public async Task Ban(CommandContext ctx, [HelpDescription("The person to ban")] DiscordMember target, [RemainingText] string reason = "No reason given.")
        {
            var user = await ctx.Guild.GetMemberAsync(target.Id);
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if(!CanExecuteCommand(out var errorReason))
            {
                await DenyBanAsync(errorReason);
                return;
            }
            async Task DenyBanAsync(string errorReason)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Red).WithDescription(errorReason)
                                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                                .WithTimestamp(DateTime.Now));
            }

            bool CanExecuteCommand(out string errorReason)
            {
                if (target == bot)
                {
                    errorReason = $"I can't ban myself!";
                    return false;
                }
                if (!ctx.Member.HasPermission(Permissions.BanMembers))
                {
                    errorReason = $"You do not have permission to ban members!";
                    return false;
                }
                if (user.IsAbove(bot))
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
                .WithColor(new DiscordColor("#0019bd"));

            var (name, url) = ctx.GetAuthor();
            var logEmbed = new DiscordEmbedBuilder()
                .WithAuthorExtension(name, url)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($":hammer: {ctx.Member.Mention} banned {target.Mention}!")
                .AddField("Infraction occured:", DateTime.UtcNow.ToString("dd/MM/yy - HH:mm UTC"))
                .AddField("Reason:", reason).AddFooter(ctx);
            try
            { 
                await DMCommand.DM(ctx, target, userBannedEmbed); 
            }
            finally
            {
                
                await ctx.Guild.BanMemberAsync(user, 7, reason);
                var loggingChannel = SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.LoggingChannel;
                var sendChannel = ctx.Guild.GetChannel(loggingChannel) ?? ctx.Channel;
                //SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.BannedMembers.Add(new BannedMember(user.Id, reason));
                await sendChannel.SendMessageAsync(embed: logEmbed);
            }



            

        }
    }
}
