using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.CommandInvocations
{
    /// <summary>
    /// Request for getting commands invoked by a specific user.
    /// </summary>
    public record GetCommandInvocationByUserRequest(ulong UserId) : IRequest<IEnumerable<CommandInvocation>>;

    /// <summary>
    /// The default handler for <see cref="GetCommandInvocationByUserRequest"/>.
    /// </summary>
    public class GetCommandInvocationByUserHandler : IRequestHandler<GetCommandInvocationByUserRequest, IEnumerable<CommandInvocation>>
    {
        private readonly GuildContext _db;
        public GetCommandInvocationByUserHandler(GuildContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<CommandInvocation>> Handle(GetCommandInvocationByUserRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<CommandInvocation> commands = await _db.CommandInvocations
                    .Where(c => c.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
            return commands;
        }
    }
}