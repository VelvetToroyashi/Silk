using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using SilkBot.Utilities;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class WaitForReaction : CommandClass
    {
        public WaitForReaction(IDbContextFactory<SilkDbContext> db) : base(db) { }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        public async Task Wait(CommandContext ctx, DiscordEmoji emoji)
        {
            using var db = GetDbContext();
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
