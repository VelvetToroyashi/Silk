using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Data.MediatR.Channels;

public static class UnlockChannel
{
    public record Request(Snowflake ChannelID) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var existing = await _db.ChannelLocks.FirstOrDefaultAsync(cl => cl.ChannelID == request.ChannelID, cancellationToken);

            if (existing is null)
                return Result.FromError(new NotFoundError("A lock wasn't found for the specified channel."));

            _db.ChannelLocks.Remove(existing);

            await _db.SaveChangesAsync(cancellationToken);

            return Result.FromSuccess();
        }
    }
}