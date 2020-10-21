using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class ReverseCommand : BaseCommandModule
    {   
        [Command]
        public async Task Reverse(CommandContext ctx, [RemainingText] string text)
        {
            if(text.Length > 500)
            {
                await ctx.RespondAsync($"Sorry, but for spam reasons I won't reverse anything over 500 characters. Your message was {ctx.Message.Content.Length} characters long.");
                return;
            }
            Array.Reverse(text.ToCharArray());
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl).WithDescription(text));
        }
    }
}
