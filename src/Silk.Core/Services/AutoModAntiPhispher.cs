using System.Buffers;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.Shared.Constants;

namespace Silk.Core.Services
{
	public sealed class AutoModAntiPhispher : IHostedService
	{
		private const string HeaderName = "X-Identity";
		private const string WebsocetUrl = "wss://phish.sinking.yachts/feed";
		private const string APIUrl = "https://phish.sinking.yachts/v2/all";
		
		private readonly HashSet<string> _domains = new();

		private readonly ILogger<AutoModAntiPhispher> _logger;

		private readonly HttpClient _client;
		private readonly ClientWebSocket _ws = new();

		private readonly CancellationTokenSource _cts = new();

		public AutoModAntiPhispher(ILogger<AutoModAntiPhispher> logger, HttpClient client)
		{
			_logger = logger;
			_client = client;
			_ws.Options.SetRequestHeader(HeaderName, StringConstants.ProjectIdentifier); // requisite for gimme-domains
		}

		public bool IsBlacklisted(string link) => _domains.Contains(link);
		
		private async Task ReceiveLoopAsync()
		{
			var stoppingToken = _cts.Token;

			var buffer = ArrayPool<byte>.Shared.Rent(32 * 1024 * 1024); // 32MB cache; should be more than sufficient for the forseeable future.

			while (!stoppingToken.IsCancellationRequested)
			{
				await _ws.ReceiveAsync(buffer, CancellationToken.None);
				var json = Encoding.UTF8.GetString(buffer);

				var payload = JObject.Parse(json);

				var command = payload["type"]!.ToString(); // "add" or "delete"
				var domains = payload["domains"]!.ToObject<string[]>()!; // An array of domains. 

				HandleWebsocketCommand(command, domains);
			}
			
			ArrayPool<byte>.Shared.Return(buffer);

			await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Silk is ready to shut down now. Bye bye...", CancellationToken.None);
		}

		private async Task<bool> GetDomainsAsync()
		{
			_logger.LogTrace(EventIds.Service, "Getting domains...");
			using var req = new HttpRequestMessage(HttpMethod.Get, APIUrl)
			{
				Headers = { { HeaderName, StringConstants.ProjectIdentifier } } // X-Identifier MUST be set or we get 403'd //
			};

			using var res = await _client.SendAsync(req);

			if (!res.IsSuccessStatusCode)
			{
				_logger.LogDebug(EventIds.Service, "Unable to get domains. ({Status}, {Reason})", res.StatusCode, res.ReasonPhrase);
				return false;
			}

			var json = await res.Content.ReadAsStringAsync();
			var payload = JsonConvert.DeserializeObject<string[]>(json)!;

			foreach (var domain in payload)
				_domains.Add(domain);
			
			_logger.LogInformation(EventIds.Service, "Retrieved {Count} phishing domains via REST", payload.Length);
			
			return true;
		}
		
		private void HandleWebsocketCommand(string? command, string[] domains)
		{
			switch (command)
			{
				case "add":
					_logger.LogDebug(EventIds.Service, "Adding {Count} new domains.", domains.Length);

					foreach (var domain in domains)
						_domains.Add(domain);
					break;

				case "delete":
					_logger.LogDebug(EventIds.Service, "Removing {Count} domains.", domains.Length);
					foreach (var domain in domains)
						_domains.Remove(domain);
					break;

				default:
					_logger.LogDebug(EventIds.Service, "Unknown command from websocket ({Command}); skipping.", command);
					break;
			}
		}
		
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (!await GetDomainsAsync())
			{
				_logger.LogCritical("Failed to retrieve domains. API - Unavailable");
				return;
			}
			
			try
			{
				await _ws.ConnectAsync(new(WebsocetUrl), CancellationToken.None);
			}
			catch (WebSocketException)
			{
				_logger.LogCritical(EventIds.Service, "Failed to establish a websocket connection. API - Unavailable");
				return;
			}

			_logger.LogInformation(EventIds.Service, "Opened websocket to phishing API.");


			_ = Task.Run(ReceiveLoopAsync);
		}


		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(EventIds.Service, "Cancellation requested, stopping service.");
			
			_cts.Cancel();
		}
	}
}