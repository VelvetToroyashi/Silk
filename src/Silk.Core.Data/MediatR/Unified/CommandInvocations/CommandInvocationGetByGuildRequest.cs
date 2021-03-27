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
    /// Gets commands invoked on a specific guild.
    /// </summary>
    public record CommandInvocationGetByGuildRequest(ulong GuildId) : IRequest<IEnumerable<CommandInvocation>>;

    /// <summary>
    /// The default handler for <see cref="CommandInvocationGetByGuildRequest"/>.
    /// </summary>
    public class CommandInvocationGetByGuildHandler : IRequestHandler<CommandInvocationGetByGuildRequest, IEnumerable<CommandInvocation>>
    {
        private readonly GuildContext _db;

        public CommandInvocationGetByGuildHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CommandInvocation>> Handle(CommandInvocationGetByGuildRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<CommandInvocation> commands = await _db.CommandInvocations
                    .Where(c => c.UserId == request.GuildId)
                    .ToListAsync(cancellationToken);
            return commands;
        }
    }
}