using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Commands.Server.Config
{
    [Group("config")]
    [RequireFlag(UserFlag.Staff)]
    public partial class ConfigCommand : BaseCommandModule
    {
        private protected readonly IMessageSender _sender;
        public ConfigCommand(IMessageSender sender)
        {
            _sender = sender;
        }

        [GroupCommand]
        public async Task ConfigWrapper(CommandContext ctx) =>
            await Config(new CommandExecutionContext(ctx.Message, ctx.Channel, ctx.Guild, _sender));

        public async Task Config(ICommandExecutionContext ctx)
        {
            await ctx.RespondAsync("Alright, what would you like to configure? (This is abstracted btw!)");
        }
    }
}