using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.CommandInvocations
{
    /// <summary>
    /// Adds a <see cref="CommandInvocation"/>.
    /// </summary>
    public record CommandInvocationAddRequest(ulong UserId, ulong? GuildId, string CommandName) : IRequest;

    /// <summary>
    /// The default handler for <see cref="CommandInvocationAddRequest"/>.
    /// </summary>
    public class CommandInvocationAddHandler : IRequestHandler<CommandInvocationAddRequest>
    {
        private readonly GuildContext _db;

        public CommandInvocationAddHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(CommandInvocationAddRequest request, CancellationToken cancellationToken)
        {
            CommandInvocation command = new() {UserId = request.UserId, GuildId = request.GuildId, CommandName = request.CommandName};
            _db.CommandInvocations.Add(command);

            await _db.SaveChangesAsync(cancellationToken);
            return new();
        }
    }
}