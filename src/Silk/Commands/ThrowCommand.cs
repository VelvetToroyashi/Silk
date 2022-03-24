using System;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Silk.Commands;

public class ThrowCommand : CommandGroup
{
    [Command("throw")]
    public Task<IResult> ThrowAsync() => throw new();
}