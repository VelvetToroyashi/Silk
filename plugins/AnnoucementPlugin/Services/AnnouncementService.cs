using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnnoucementPlugin.Database;
using AnnoucementPlugin.Database.MediatR;
using AnnoucementPlugin.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnnoucementPlugin.Services
{
	// We can't use IHostedService/BackgroundService (which implements IHostedService) because there's no gaurantee that this is not hot-plugged. //
	public sealed class AnnouncementService
	{
		private readonly IMediator _mediator;
		private readonly AsyncTimer _announcementTimer;
		private readonly IMessageDispatcher _dispatcher;
		private readonly ILogger<AnnouncementService> _logger;
		private readonly List<AnnouncementModel> _announcements = new();

		/// <summary>
		/// Minimum threshold time required for an announcement to be saved to the database. 
		/// </summary>
		private readonly TimeSpan _minExpirationDatabaseThreshold = TimeSpan.FromSeconds(5);
		
		private bool _started;

		public AnnouncementService(ILogger<AnnouncementService> logger, IMessageDispatcher dispatcher, IMediator mediator)
		{
			_logger = logger;
			_dispatcher = dispatcher;
			_mediator = mediator;
			_announcementTimer = new(OnTick, TimeSpan.FromSeconds(1), yieldToTask: false);
		}

		public void Start()
		{
			if (_started)
				throw new InvalidOperationException("This service has already been started");
			
			_started = true;
			_announcementTimer.Start();
		}

		public async ValueTask ScheduleAnnouncementAsync(string content, ulong guild, ulong channel, TimeSpan expiration)
		{
			if (expiration > _minExpirationDatabaseThreshold)
			{
				var nonCachedAnnouncement = new AnnouncementModel
				{
					AnnouncementMessage = content,
					GuildId = guild,
					ChannelId = channel,
					ScheduledFor = DateTime.UtcNow + expiration
				};
				
				_announcements.Add(nonCachedAnnouncement);
			}
			else
			{
				var dbBackedAnnouncement = await _mediator.Send(new CreateAnnouncementRequest(content, guild, channel, DateTime.UtcNow + expiration));
				
				_announcements.Add(dbBackedAnnouncement);
			}
		}

		private async Task OnTick()
		{
			foreach (var announcement in _announcements)
			{
				if (announcement.ScheduledFor <= DateTime.UtcNow) 
				{
					var res = await _dispatcher.DispatchMessage(announcement.GuildId, announcement.ChannelId, announcement.AnnouncementMessage);
					
					if (!res.Succeeded)
					{
						_logger.LogWarning("An announcement failed to send. Destination: {Channel} on {Guild} failed with response of {Response}",
							announcement.ChannelId, announcement.GuildId, res.ErrorType);
					}
					else
					{
						_logger.LogDebug("Successfully dispatched announcement.");
						await _mediator.Send(new RemoveAnnouncementRequest(announcement));
					}
					
					/*
					 TODO: Handle errors
					 TODO: add AddedFrom channel to announcement model 
					 TODO: I forgor
					 */
				}
			}
		}
	}
}