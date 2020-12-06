using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class ClearCommand : BaseCommandModule
    {
        [Command("Clear")]
        [HelpDescription(
            "Cleans all messages from all users. \n Note, clearing more than 50 messages will lock the channel during bulk deletion. Coming soon:tm:",
            "!clear 20")]
        public async Task Clear(CommandContext ctx, [HelpDescription("The number of messages to clear.")]
            int messages = 5)
        {
            // Anyone who's got permission to manage channels might not be staff, so adding [RequireFlag(UserFlag.Staff)] needlessly permwalls it. //
            if (!ctx.Member.HasPermission(Permissions.ManageChannels))
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                              .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                                              .WithColor(DiscordColor.Red)
                                              .WithDescription(
                                                  $"Sorry, but you need to be able to manage messages to use this command!"));
                return;
            }

            IReadOnlyList<DiscordMessage> queriedMessages = await ctx.Channel.GetMessagesAsync(messages + 1);
            await ctx.Channel.DeleteMessagesAsync(queriedMessages,
                $"{ctx.User.Username}{ctx.User.Discriminator} called clear command.");

            DiscordMessage deleteConfirmationMessage = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($"Cleared {messages} messages!")
                .WithFooter(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));
            //Change to whatever.//
            await Task.Delay(5000);
            if (deleteConfirmationMessage is not null)
                await ctx.Channel.DeleteMessageAsync(deleteConfirmationMessage);
        }
    }
}