using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Silk;

public class SilkTreeNameResolver : ITreeNameResolver
{

    public async Task<Result<string>> GetTreeNameAsync(IOperationContext context, CancellationToken ct = default)
    {
        if (context is MessageContext)
            return Result<string>.FromSuccess(null!);

        else return Result<string>.FromSuccess("silk_slash_tree");
    }
}
