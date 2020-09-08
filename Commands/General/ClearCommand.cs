using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    public class ClearCommand : BaseCommandModule
    {
        [Command("Clear")]
        [HelpDescription("Cleans all messages from all users. \n Note, clearing more than 50 messages will lock the channel during bulk deletion. Coming soon:tm:", "!clear 20")]

        public async Task Clear(CommandContext ctx, [HelpDescription("The number of messages to clear.")] int messages = 5)
        {
            if (!ctx.Member.HasPermission(Permissions.ManageChannels))
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
               .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
               .WithColor(DiscordColor.Red)
               .WithDescription($"Sorry, but you need to be able to manage messages to use this command!")
               .WithFooter(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl)
               .WithTimestamp(DateTime.Now));
                return;
            }
            var lockoutChannel = ctx.Channel;
            ulong messageID = ctx.Message.Id;
            var actualMessageCount = lockoutChannel.GetMessagesBeforeAsync(ctx.Message.Id, messages).Result.Count();
            var queryConfirmationMessage = ctx.Message;
            if (messages > 50)
            {

                await ctx.TriggerTypingAsync();
                await Task.Delay(2000);
                queryConfirmationMessage = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.Yellow)
                .WithDescription($"Initiated bulk delete. Querying {actualMessageCount} messages.")
                .WithFooter(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl));
                messageID = queryConfirmationMessage.Id;
                await Task.Delay(4000);
            }



            await ctx.Channel.DeleteMessagesAsync(ctx.Channel.GetMessagesBeforeAsync(messageID, messages).Result);
            await lockoutChannel.GetMessageAsync(queryConfirmationMessage.Id).Result.DeleteAsync();
            var deleteConfirmationMessage = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($"Cleared {actualMessageCount + 1} messages!")
                .WithFooter(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));
            //Change to whatever.//
            await Task.Delay(5000);
            await ctx.Channel.DeleteMessageAsync(deleteConfirmationMessage);


        }

        public async Task SyncPermissions(CommandContext ctx, DiscordChannel channel)
        {
            foreach (var ow in channel.Parent.PermissionOverwrites)
            {
                var role = await ow.GetRoleAsync();
                await channel.AddOverwriteAsync(role, ow.Allowed, ow.Denied, $"Syncing with Parent per request from {ctx.User}");
            }
        }

    }
}
