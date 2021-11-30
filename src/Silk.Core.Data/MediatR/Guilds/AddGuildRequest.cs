using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds
{
	/// <summary>
	///     Request for adding a <see cref="GuildEntity"/> to the database.
	/// </summary>
	/// <param name="GuildId">The Id of the Guild</param>
	/// <param name="Prefix">The prefix for the Guild</param>
	public sealed record AddGuildRequest(ulong GuildId, string Prefix) : IRequest<GuildEntity>;

	/// <summary>
	///     The default handler for <see cref="AddGuildRequest"/>.
	/// </summary>
	public sealed class AddGuildHandler : IRequestHandler<AddGuildRequest, GuildEntity>
	{
		private readonly GuildContext _db;
		public AddGuildHandler(GuildContext db)
		{
			_db = db;
		}

		public async Task<GuildEntity> Handle(AddGuildRequest request, CancellationToken cancellationToken)
		{
			GuildEntity guild = new()
			{
				Id = request.GuildId,
				Prefix = request.Prefix,
				Configuration = new() { GuildId = request.GuildId },
				ModConfig = new() { GuildId = request.GuildId },
			};

			_db.Guilds.Add(guild);
			await _db.SaveChangesAsync(cancellationToken);

			return guild;
		}
	}
}