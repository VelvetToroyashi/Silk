using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using SilkBot.Utilities;
using System;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class WaitForReaction : CommandClass
    {
        public WaitForReaction(IDbContextFactory<SilkDbContext> db) : base(db) { }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        public async Task Wait(CommandContext ctx)
        {
            var Interactivity = ctx.Client.GetInteractivity();

            DiscordMessage msg = ctx.Message;

            DiscordEmoji acceptEmoji = DiscordEmoji.FromUnicode("🆗");

            var reactionResult = await Interactivity.WaitForReactionAsync(
                                 x => x.Emoji == acceptEmoji,
                                 msg, ctx.Member, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        }
    }
}
