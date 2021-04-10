using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.CommandInvocations
{
    /// <summary>
    ///     Request for adding a <see cref="CommandInvocation" />.
    /// </summary>
    public record AddCommandInvocationRequest(ulong UserId, ulong? GuildId, string CommandName) : IRequest;

    /// <summary>
    ///     The default handler for <see cref="AddCommandInvocationRequest" />.
    /// </summary>
    public class AddCommandInvocationHandler : IRequestHandler<AddCommandInvocationRequest>
    {
        private readonly GuildContext _db;

        public AddCommandInvocationHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(AddCommandInvocationRequest request, CancellationToken cancellationToken)
        {
            CommandInvocation command = new() {UserId = request.UserId, GuildId = request.GuildId, CommandName = request.CommandName};
            _db.CommandInvocations.Add(command);

            await _db.SaveChangesAsync(cancellationToken);
            return new();
        }
    }
}