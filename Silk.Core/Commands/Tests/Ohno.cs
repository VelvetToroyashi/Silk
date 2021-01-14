using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Silk.Core.Commands.Tests
{
    public class Ohno
    {
        [Command]
        public async Task O(CommandContext ctx)
        {
            DiscordMessageBuilder builder = new();
            builder.WithContent("This is with the message builder!").WithReply(ctx.Message.Id, true);
            await ctx.RespondAsync(builder);

        }
    }
}