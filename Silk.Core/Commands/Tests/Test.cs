using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Silk.Core.Commands.Tests
{
    [Group]
    public class Test : BaseCommandModule
    {
        [Command]
        public async Task A(CommandContext c) => await c.RespondAsync("Group: Test | Command : A");

        [Group]
        public class Test2 : BaseCommandModule
        {
            [Command] public async Task A(CommandContext c) => await c.RespondAsync("Group: Test2 | Command A");
        }
    }

    public class Test2 : BaseCommandModule
    {
        [Command]
        public async Task B(CommandContext c) => await c.RespondAsync("Group: Test2 | Command B");
    }
}