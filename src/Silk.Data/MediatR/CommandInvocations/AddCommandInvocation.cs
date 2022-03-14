using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.CommandInvocations;

public static class AddCommandInvocation
{
    /// <summary>
    /// Request for adding a <see cref="CommandInvocationEntity" />.
    /// </summary>
    public sealed record Request(string CommandName) : IRequest;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            CommandInvocationEntity command = new()
            {
                CommandName    = request.CommandName,
                InvocationTime = DateTime.UtcNow
            };

            _db.CommandInvocations.Add(command);

            await _db.SaveChangesAsync(cancellationToken);
            return new();
        }
    }
}