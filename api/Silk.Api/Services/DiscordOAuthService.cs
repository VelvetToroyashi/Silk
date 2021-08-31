using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silk.Api.Helpers;

namespace Silk.Api.Services
{
	public class DiscordOAuthService
	{
		private readonly HttpClient _client;
		private readonly ApiSettings _settings;
		private ILogger<DiscordOAuthService> _logger;
		public DiscordOAuthService(HttpClient client, IOptions<ApiSettings> settings, ILogger<DiscordOAuthService> logger)
		{
			_client = client;
			_logger = logger;
			_settings = settings.Value;
		}

		public async Task<(bool Authenticated, ulong Id)> VerifyDiscordApplicationAsync(string id, string secret)
		{
			// Attempt to generate a bearer token to verify //
			var req = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/v9/oauth2/token");

			var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{id}:{secret}"));
			req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

			req.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["grant_type"] = "client_credentials",
				["scope"] = "identify"
			});
			
			req.Content.Headers.ContentType = new("application/x-www-form-urlencoded");
			
			var res = await _client.SendAsync(req);
			
			// Couldn't generate a bearer token //
			if (res.StatusCode is not HttpStatusCode.OK)
				return (false, 0);

			var obj = JObject.Parse(await res.Content.ReadAsStringAsync());

			req.Headers.Authorization = null;

			_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {obj["access_token"]}");
			//req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", obj["access_token"]!.ToString());

			var ret = await _client.GetAsync("https://discord.com/api/v9/oauth2/@me");
			_client.DefaultRequestHeaders.Remove("Authorization");
			
			var user = (ulong)JObject.Parse(await ret.Content.ReadAsStringAsync()!)["user"]!["id"];
			
			// Revoke the token; it's not needed anymore //
			
			req.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", obj["access_token"]!.ToString()) });
			req.RequestUri = new("https://discord.com/api/v9/oauth2/token/revoke");

			await _client.SendAsync(req);
			
			return (true, user);
		}
	}
}