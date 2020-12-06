using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Exceptions;
using SilkBot.Extensions;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class BotStatus : BaseCommandModule
    {
        [Command]
        public async Task Status(CommandContext ctx)
        {
            string activity = ctx.Client.CurrentUser.Presence.Activity.Name;
            DiscordEmbedBuilder status = new DiscordEmbedBuilder()
                                         .WithColor(activity == null
                                             ? DiscordColor.CornflowerBlue
                                             : DiscordColor.SapGreen)
                                         .WithTitle("Bot status:")
                                         .WithDescription(activity ?? "I'm not currently playing anything :).")
                                         .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                         .WithTimestamp(DateTime.Now);

            await ctx.RespondAsync(embed: status);
        }

        [Command, Hidden]
        public async Task Status(CommandContext ctx, string status)
        {
            var authUsers = new ulong[] {209279906280898562, 135747025000988672, 265096437937864705};
            if (!authUsers.Any(id => id == ctx.User.Id))
                throw new UnauthorizedUserException("Sorry, but only Velvet, Morgan, and Tami are allowed to change my status~");
            else
            {
                if (status is "clear")
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.SapGreen)
                                                .WithDescription("Bot status has been cleared!")
                                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                                .WithTimestamp(DateTime.Now);

                    await ctx.Client.UpdateStatusAsync();
                    await ctx.RespondAsync(embed: embed);
                }

                await ctx.Client.UpdateStatusAsync(new DiscordActivity
                    {ActivityType = ctx.Client.CurrentUser.Presence.Activity.ActivityType, Name = status});
            }
        }


        [Command, Hidden]
        public async Task Status(CommandContext ctx, ActivityType type, [RemainingText] string status)
        {
            var authUsers = new ulong[] {209279906280898562, 135747025000988672, 265096437937864705};
            if (!authUsers.Any(id => id == ctx.User.Id))
            {
                throw new UnauthorizedUserException("You are not permitted to use this command!");
            }
            else
            {
                var update = status.ToLower() is "clear" ? 
                     ctx.Client.UpdateStatusAsync() : 
                     ctx.Client.UpdateStatusAsync(new DiscordActivity(status, type), idleSince: DateTime.Now);

                await update;
                await ctx.RespondAsync("Done!");
            }
        }

        
        [Command, Hidden, Priority(2)]
        public async Task Status(CommandContext ctx, string type, [RemainingText] string status)
        {
            if (!ctx.Channel.IsPrivate) await ctx.Channel.DeleteMessageAsync(ctx.Message);

            var authUsers = new ulong[] {209279906280898562, 135747025000988672, 265096437937864705};
            if (!authUsers.Any(id => id == ctx.User.Id))
            {
                throw new UnauthorizedUserException("You are not permitted to use this command!");
            }
            else
            {
                if (status.ToLower() is "clear")
                {
                    await ctx.Client.UpdateStatusAsync();
                    await ctx.RespondAsync("Done!");
                }

                else
                {
                    if (!Enum.TryParse(typeof(ActivityType), type, true, out var activity))
                        await Status(ctx, status);
                    else
                    {
                        await ctx.Client.UpdateStatusAsync(new DiscordActivity(status, (ActivityType) activity),
                            idleSince: DateTime.Now);
                        DiscordMessage msg = await ctx.RespondAsync(
                            embed: EmbedHelper.CreateEmbed(ctx, "Status", $"Successfully set status to {status}!"));
                        await Task.Delay(3000);
                        await ctx.Channel.DeleteMessageAsync(msg);
                    }
                }
            }
        }
    }
}