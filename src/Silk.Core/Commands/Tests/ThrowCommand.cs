using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Silk.Core.Commands.Tests
{
    public class ThrowCommand : BaseCommandModule
    {
        [Command]
        public async Task Throw(CommandContext ctx) => throw new InvalidOperationException("No.");
    }
}