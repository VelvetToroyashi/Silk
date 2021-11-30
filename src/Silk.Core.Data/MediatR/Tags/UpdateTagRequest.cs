using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Tags
{
	/// <summary>
	///     Request to update a <see cref="TagEntity"/>.
	/// </summary>
	/// <param name="Name">The Name of the Tag</param>
	/// <param name="GuildId">The Id of the Guild</param>
	public record UpdateTagRequest(string Name, ulong GuildId) : IRequest<TagEntity>
	{
		public ulong? OwnerId { get; init; }
		public string? NewName { get; init; }
		public int? Uses { get; init; }
		public string? Content { get; init; }
		public List<TagEntity>? Aliases { get; init; }
	}

	/// <summary>
	///     The default handler for <see cref="T:Silk.Core.Data.MediatR.Tags.UpdateTagRequest"/>.
	/// </summary>
	public sealed class UpdateTagHandler : IRequestHandler<UpdateTagRequest, TagEntity>
	{
		private readonly GuildContext _db;
		public UpdateTagHandler(GuildContext db)
		{
			_db = db;
		}

		public async Task<TagEntity> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
		{
			TagEntity tag = await _db.Tags
				.Include(t => t.Aliases)
				.FirstAsync(t => t.Name == request.Name && t.GuildId == request.GuildId, cancellationToken);

			tag.Uses = request.Uses ?? tag.Uses;
			tag.Name = request.NewName ?? tag.Name;
			tag.OwnerId = request.OwnerId ?? tag.OwnerId;
			tag.Content = request.Content ?? tag.Content;
			tag.Aliases = request.Aliases ?? tag.Aliases;
			await _db.SaveChangesAsync(cancellationToken);
			_db.ChangeTracker.Clear();

			if (tag.Aliases?.Any() ?? false)
				foreach (var alias in request.Aliases!)
					alias.Content = tag.Content;

			await _db.SaveChangesAsync(cancellationToken);

			return tag;
		}
	}
}