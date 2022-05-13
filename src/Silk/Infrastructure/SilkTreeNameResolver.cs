using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Silk;

public class SilkTreeNameResolver : ITreeNameResolver
{

    public async Task<Result<(string TreeName, bool AllowDefaultTree)>> GetTreeNameAsync(ICommandContext context, CancellationToken ct = default)
    {
        if (context is MessageContext)
            return Result<(string, bool)>.FromSuccess((null, true));

        else return Result<(string, bool)>.FromSuccess(("silk_slash_tree", false));
    }
}