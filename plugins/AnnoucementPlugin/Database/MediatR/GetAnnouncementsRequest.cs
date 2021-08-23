using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnnoucementPlugin.Database.MediatR
{
	public sealed record GetAnnouncementsRequest : IRequest<AnnouncementModel[]>;

	public sealed class GetAnnouncementsHandler : IRequestHandler<GetAnnouncementsRequest, AnnouncementModel[]>
	{
		private readonly AnnouncementContext _db;
		public GetAnnouncementsHandler(AnnouncementContext db) => _db = db;

		public Task<AnnouncementModel[]> Handle(GetAnnouncementsRequest request, CancellationToken cancellationToken)
			=> _db.Announcements.ToArrayAsync(cancellationToken);
	}
}