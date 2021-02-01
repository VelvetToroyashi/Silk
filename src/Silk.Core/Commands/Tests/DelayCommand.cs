using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Constants;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Tests
{
    [RequireOwner]
    [Category(Categories.Dev)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class DelayCommand : BaseCommandModule
    {
        [Command]
        [Description("Delay the execution of a command!")]
        public async Task Delay(CommandContext ctx, string command, TimeSpan delay, [RemainingText] string? parameters)
        {
            bool exists = ctx.CommandsNext.RegisteredCommands.TryGetValue(command, out Command? cmd);
            if (!exists)
            {
                await ctx.RespondAsync("That isn't a valid command!");
                return;
            }

            await ctx.Message.CreateReactionAsync(Emojis.EConfirm);
            await Task.Delay(delay);

            CommandContext context = ctx.CommandsNext.CreateContext(ctx.Message, ctx.Prefix, cmd, parameters);
            await ctx.CommandsNext.ExecuteCommandAsync(context);
        }
    }
}