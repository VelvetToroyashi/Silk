using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace AnnoucementPlugin.Database.MediatR
{
    public sealed record RemoveAnnouncementRequest(AnnouncementModel Announcement) : IRequest;

    public sealed class RemoveAnnouncemtHandler : IRequestHandler<RemoveAnnouncementRequest>
    {
        private readonly AnnouncementContext _db;
        public RemoveAnnouncemtHandler(AnnouncementContext db) => _db = db;

        public async Task<Unit> Handle(RemoveAnnouncementRequest request, CancellationToken cancellationToken)
        {
            _db.Announcements.Remove(request.Announcement);

            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}