using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AnnoucementPlugin.Database
{
	public sealed class AnnouncementContext : IHostedService
	{
		
		public async Task StartAsync(CancellationToken cancellationToken) { }
		public async Task StopAsync(CancellationToken cancellationToken) { }
	}
}