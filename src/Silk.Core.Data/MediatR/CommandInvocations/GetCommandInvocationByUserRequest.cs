using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.CommandInvocations
{
	/// <summary>
	///     Request for getting commands invoked by a specific user.
	/// </summary>
	public record GetCommandInvocationByUserRequest(ulong UserId) : IRequest<IEnumerable<CommandInvocationEntity>>;

	/// <summary>
	///     The default handler for <see cref="GetCommandInvocationByUserRequest" />.
	/// </summary>
	public class GetCommandInvocationByUserHandler : IRequestHandler<GetCommandInvocationByUserRequest, IEnumerable<CommandInvocationEntity>>
    {
        private readonly GuildContext _db;

        public GetCommandInvocationByUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CommandInvocationEntity>> Handle(GetCommandInvocationByUserRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<CommandInvocationEntity> commands = await _db.CommandInvocations
                                                                     .Where(c => c.UserId == request.UserId)
                                                                     .ToListAsync(cancellationToken);

            return commands;
        }
    }
}