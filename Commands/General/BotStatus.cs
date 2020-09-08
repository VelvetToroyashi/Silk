using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Exceptions;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    public class BotStatus : BaseCommandModule
    {



        [Command("Status")]
        public async Task Status(CommandContext ctx)
        {

            var activity = ctx.Client.CurrentUser.Presence.Activity.Name;
            var status = new DiscordEmbedBuilder()
                .WithColor(activity == null ? DiscordColor.CornflowerBlue : DiscordColor.SapGreen)
                .WithTitle("Bot status:")
                .WithDescription(activity ?? "The bot's status is not set.")
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            await ctx.RespondAsync(embed: status);



        }


        public async Task Status(CommandContext ctx, string status)
        {
            var authUsers = new ulong[] { 209279906280898562, 135747025000988672, 265096437937864705 };
            if (!authUsers.Any(id => id == ctx.User.Id))
            {
                throw new UnauthorizedUserException("You're not authorized to use this command!");
            }
            else
            {
                if (status.Equals("clear"))
                {
                    var embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SapGreen)
                    .WithDescription("Bot status has been cleared!")
                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                    await ctx.Client.UpdateStatusAsync();
                    await ctx.RespondAsync(embed: embed);
                }
                await ctx.Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ctx.Client.CurrentUser.Presence.Activity.ActivityType, Name = status });
            }
        }



        [Hidden, Command("status")]
        public async Task Status(CommandContext ctx, ActivityType type, [RemainingText] string status)
        {


            var authUsers = new ulong[] { 209279906280898562, 135747025000988672, 265096437937864705 };
            if (!authUsers.Any(id => id == ctx.User.Id))
            {
                throw new UnauthorizedUserException("You are not permitted to use this command!");
            }
            else
            {
                if (status.Equals("clear"))
                {
                    var embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SapGreen)
                    .WithDescription("Bot status has been cleared!")
                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                    await ctx.Client.UpdateStatusAsync();
                    await ctx.RespondAsync(embed: embed);
                }
                else
                {
                    await ctx.Client.UpdateStatusAsync(new DiscordActivity(status, type), idleSince: DateTime.Now);
                }
            }
        }

        [Hidden, Command("status")]
        public async Task Status(CommandContext ctx, string type, [RemainingText] string status)
        {
            if (!ctx.Channel.IsPrivate)
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message);
            }

            var authUsers = new ulong[] { 209279906280898562, 135747025000988672, 265096437937864705 };
            if (!authUsers.Any(id => id == ctx.User.Id))
            {
                throw new UnauthorizedUserException("You are not permitted to use this command!");
            }
            else
            {
                if (status.Equals("clear"))
                {
                    var embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SapGreen)
                    .WithDescription("Bot status has been cleared!")
                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                    await ctx.Client.UpdateStatusAsync();
                    await ctx.RespondAsync(embed: embed);
                }

                else
                {
                    if (!Enum.TryParse(typeof(ActivityType), type, true, out var activity))
                    {
                        await Status(ctx, status);
                        return;
                    }
                    else
                    {
                        await ctx.Client.UpdateStatusAsync(new DiscordActivity(status, (ActivityType)activity), idleSince: DateTime.Now);
                        var msg = await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, "Status", $"Successfully set status to {status}!"));
                        await Task.Delay(3000);
                        await ctx.Channel.DeleteMessageAsync(msg);
                    }

                }
            }
        }

    }
}
