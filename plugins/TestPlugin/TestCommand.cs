using System.Drawing;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Plugins;
using Remora.Results;

namespace TestPlugin;

[Group("config")]
public class TestCommand : CommandGroup
{
    [Group("edit")]
    public class EditCommands : CommandGroup
    {
        [Command("test")]
        public async Task<IResult> Test() => Result.FromSuccess();
    }
}