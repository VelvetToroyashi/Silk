using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Silk.Commands;

public class TestCommand : CommandGroup
{
    [Command("throw")]
    public async Task<Result> Test()
    {
        throw new();
    }
}