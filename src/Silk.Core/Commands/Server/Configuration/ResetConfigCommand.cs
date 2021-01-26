using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Silk.Core.Commands.Server.Configuration
{
    //[Group("config")]
    public class ResetConfigCommand : BaseCommandModule
    {
        [Command]
        public async Task Reset(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            var confirmationCode = new Random((int) ctx.Message.Id).Next(1000, 10000);
            builder.WithReply(ctx.Message.Id, true);
            builder.WithContent($"**All settings will be reset** (This does not include server prefix) | Are you sure? Type `{confirmationCode}` to confirm. Type cancel to cancel");
            await ctx.RespondAsync(builder);

            var interactivity = ctx.Client.GetInteractivity();
            var result = await interactivity.WaitForMessageAsync(m => m.Content.Equals("cancel", StringComparison.CurrentCultureIgnoreCase) || m.Content.Equals(confirmationCode));




        }
    }
}