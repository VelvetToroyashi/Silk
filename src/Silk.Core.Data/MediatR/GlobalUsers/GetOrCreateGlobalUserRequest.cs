using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.GlobalUsers
{
	/// <summary>
	///     Request for getting a user who's data is stored globally, or creates it if it does not exist.
	/// </summary>
	public record GetOrCreateGlobalUserRequest(ulong UserId) : IRequest<GlobalUserEntity>;

	/// <summary>
	///     The default handler for <see cref="GetOrCreateGlobalUserRequest"/>.
	/// </summary>
	public class GetOrCreateGlobalUserHandler : IRequestHandler<GetOrCreateGlobalUserRequest, GlobalUserEntity>
	{
		private readonly GuildContext _db;

		public GetOrCreateGlobalUserHandler(GuildContext db)
		{
			_db = db;
		}

		public async Task<GlobalUserEntity> Handle(GetOrCreateGlobalUserRequest request, CancellationToken cancellationToken)
		{
			GlobalUserEntity? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
			user ??= new()
			{
				Id = request.UserId,
				LastCashOut = DateTime.MinValue
			};
			await _db.SaveChangesAsync(cancellationToken);
			return user;
		}
	}
}