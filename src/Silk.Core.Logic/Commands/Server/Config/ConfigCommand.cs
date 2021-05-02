using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities;

namespace Silk.Core.Logic.Commands.Server.Config
{
    [Hidden]
    [RequireGuild]
    [Group("config")]
    [RequireFlag(UserFlag.Staff)]
    public sealed partial class ConfigCommand : BaseCommandModule
    {
        [GroupCommand]
        public async Task ConfigWrapper(CommandContext ctx) =>
            await ctx.RespondAsync($"See `{ctx.Prefix}help config` instead.");
    }
}