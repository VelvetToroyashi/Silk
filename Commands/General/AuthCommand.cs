using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot
{
    public class AuthCommand : BaseCommandModule
    {
        public static string Key { get; private set; }

        [Hidden]
        [Command]
        [RequireOwner]
        public async Task Auth(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            Key = Guid.NewGuid().ToString();
            Console.WriteLine("Authenthication key: " + Key);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithTitle($"Check the console, {ctx.Member.DisplayName.Split(' ').First()} !")
                .WithColor(DiscordColor.CornflowerBlue)
                .WithDescription("Authenthication token valid for 1 use. Token printed to console")
                .WithFooter("Silk")
                .WithTimestamp(DateTime.Now));
            await Task.Delay(5000);
            await ctx.Channel.GetMessagesAfterAsync(ctx.Message.Id).Result.First().DeleteAsync();
        }

        public static async void ClearKeyAsync() => await Task.Run(() => Key = null);
    }
}