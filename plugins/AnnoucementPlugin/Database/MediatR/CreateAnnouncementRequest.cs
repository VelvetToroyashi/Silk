using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace AnnoucementPlugin.Database.MediatR
{
	public sealed record CreateAnnouncementRequest(string Content, ulong GuildId, ulong ChannelId, DateTime ScheduledFor) : IRequest<AnnouncementModel>;
	
	public sealed class CreateAnnouncementHandler : IRequestHandler<CreateAnnouncementRequest, AnnouncementModel>
	{
		private readonly AnnouncementContext _db;
		public CreateAnnouncementHandler(AnnouncementContext db) => _db = db;
		
		public async Task<AnnouncementModel> Handle(CreateAnnouncementRequest request, CancellationToken cancellationToken)
		{
			var announcement = new AnnouncementModel()
			{
				AnnouncementMessage = request.Content,
				GuildId = request.GuildId,
				ChannelId = request.ChannelId,
				ScheduledFor = request.ScheduledFor
			};

			_db.Announcements.Add(announcement);

			await _db.SaveChangesAsync(cancellationToken);

			return announcement;
		}
	}
}