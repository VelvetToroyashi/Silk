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
    /// Request for getting commands invoked on a specific guild.
    /// </summary>
    public record GetCommandInvocationByGuildRequest(ulong GuildId) : IRequest<IEnumerable<CommandInvocation>>;

    /// <summary>
    /// The default handler for <see cref="GetCommandInvocationByGuildRequest"/>.
    /// </summary>
    public class GetCommandInvocationByGuildHandler : IRequestHandler<GetCommandInvocationByGuildRequest, IEnumerable<CommandInvocation>>
    {
        private readonly GuildContext _db;

        public GetCommandInvocationByGuildHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CommandInvocation>> Handle(GetCommandInvocationByGuildRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<CommandInvocation> commands = await _db.CommandInvocations
                    .Where(c => c.UserId == request.GuildId)
                    .ToListAsync(cancellationToken);
            return commands;
        }
    }
}