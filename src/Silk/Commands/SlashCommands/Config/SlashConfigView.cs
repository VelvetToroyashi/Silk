using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;
using Silk.Extensions.Remora;

namespace Silk.Commands.SlashCommands.Config;

public partial class SlashConfig
{
    [Group("view")]
    public partial class View : CommandGroup
    {
        [Command("all")]
        public async Task<IResult> ViewAllAsync() => throw new NotImplementedException();
    }
}
