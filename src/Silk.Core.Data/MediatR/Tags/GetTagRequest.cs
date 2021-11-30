using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Tags
{
	/// <summary>
	///     Request to get a <see cref="TagEntity"/>, or null if it doesn't exist.
	/// </summary>
	public record GetTagRequest(string Name, ulong GuildId) : IRequest<TagEntity?>;

	/// <summary>
	///     The default handler for <see cref="GetTagRequest"/>.
	/// </summary>
	public class GetTagHandler : IRequestHandler<GetTagRequest, TagEntity?>
	{
		private readonly GuildContext _db;

		public GetTagHandler(GuildContext db)
		{
			_db = db;
		}

		public async Task<TagEntity?> Handle(GetTagRequest request, CancellationToken cancellationToken)
		{
			TagEntity? tag = await _db.Tags
				.Include(t => t.OriginalTag)
				.Include(t => t.Aliases)
				.AsSplitQuery()
				.FirstOrDefaultAsync(t =>
					t.Name.ToLower() == request.Name.ToLower()
					&& t.GuildId == request.GuildId, cancellationToken);

			return tag;
		}
	}
}