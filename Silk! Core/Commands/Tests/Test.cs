using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    [Group("Foo")]
    public class Test : BaseCommandModule
    {
        [GroupCommand]
        public async Task A(CommandContext c) => await c.RespondAsync("Group Foo, command A");
        [Command("Heck")]
        public async Task B(CommandContext c) => await c.RespondAsync("Group Foo, command B");

        [Group("Bar")]
        public class OwO : BaseCommandModule
        {
            [GroupCommand]
            public async Task A(CommandContext c) => await c.RespondAsync("Group Bar, command A");
            [Command]
            public async Task B(CommandContext c) => await c.RespondAsync("Group Bar, command B");
        }
    }
}
