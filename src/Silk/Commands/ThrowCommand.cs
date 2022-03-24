using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;
using Silk.Commands.Conditions;

namespace Silk.Commands;

public class ThrowCommand : CommandGroup
{
    [Command("throw")]
    [RequireTeamOrOwner]
    [Description("A command mainly used for debugging.")]
    public Task<IResult> ThrowAsync() => throw new();
}