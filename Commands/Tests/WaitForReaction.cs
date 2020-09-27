using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class WaitForReaction : BaseCommandModule
    {
        public async Task Wait(CommandContext ctx, DiscordEmoji emoji)
        {
            var interactivity = ctx.Client.GetInteractivity();
            await ctx.Message.CreateReactionAsync(emoji);
            var msg = await interactivity.WaitForReactionAsync(a => a.Emoji == emoji, ctx.Message, ctx.User);

            if (!msg.TimedOut)
            {
                await ctx.RespondAsync("Success");
            }
        }
    }
}
