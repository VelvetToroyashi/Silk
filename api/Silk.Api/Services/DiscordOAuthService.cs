using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silk.Api.Helpers;

namespace Silk.Api.Services
{
	public class DiscordOAuthService
	{
		private readonly HttpClient _client;
		private readonly ApiSettings _settings;
		public DiscordOAuthService(HttpClient client, IOptions<ApiSettings> settings)
		{
			_client = client;
			_settings = settings.Value;
		}

		public async Task<(bool Authenticated, ulong Id)> VerifyDiscordApplicationAsync(string id, string secret)
		{
			// Attempt to generate a bearer token to verify //
			var req = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/v9/oauth2/token");
			req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			req.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["client_id"] = id,
				["client_secret"] = secret,
				["grant_type"] = "client_credentials",
			});

			var res = await _client.SendAsync(req);
			
			// Couldn't generate a bearer token //
			if (res.StatusCode is not HttpStatusCode.OK)
				return (false, 0);

			var obj = JObject.Parse(await res.Content.ReadAsStringAsync());

			var auth = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{id}:{secret}"));
			
			var user = (ulong)JObject.Parse(await _client.GetStringAsync("https://discord.com/api/v9/oauth2/@me"))["user"]!["id"];
			
			// Revoke the token; it's not needed anymore //
			req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
			req.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", obj["access_token"]!.ToString()) });
			req.RequestUri = new("https://discord.com/api/v9/oauth2/token/revoke");

			await _client.SendAsync(req);
			
			return (true, user);
		}
	}
}