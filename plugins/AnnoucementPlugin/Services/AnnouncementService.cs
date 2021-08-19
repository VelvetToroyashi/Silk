using System;
using System.Threading.Tasks;
using AnnoucementPlugin.Utilities;
using Microsoft.Extensions.Logging;

namespace AnnoucementPlugin.Services
{
	public sealed class AnnouncementService
	{
		private readonly AsyncTimer _announcementTimer;
		private readonly IMessageDispatcher _dispatcher;
		private readonly ILogger<AnnouncementService> _logger;

		public AnnouncementService(ILogger<AnnouncementService> logger, IMessageDispatcher dispatcher)
		{
			_logger = logger;
			_dispatcher = dispatcher;
			_announcementTimer = new(OnTick, TimeSpan.FromSeconds(1), yieldToTask: false);
		}

		public async Task StartAsync()
		{
			_announcementTimer.Start();
		}

		private async Task OnTick() { }
	}
}