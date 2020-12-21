using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Silk_Items.Entities;
using Silk_Items.Tools;
using SilkBot.Database.Models;
using SilkBot.Extensions;

namespace SilkBot.Commands.Tests
{
    [Group]
    public class Test : BaseCommandModule
    {
        [Command]
        public async Task A(CommandContext c) => await c.RespondAsync("Group: Test | Command : A");

        [Group]
        public partial class Test2 : BaseCommandModule { [Command] public async Task A(CommandContext c) =>  await c.RespondAsync("Group: Test2 | Command A"); }
    }

    public partial class Test2 : BaseCommandModule
    {
        [Command]
        public async Task B(CommandContext c) => await c.RespondAsync("Group: Test2 | Command B");
    }
}