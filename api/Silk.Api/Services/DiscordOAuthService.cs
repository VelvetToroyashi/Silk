using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
		
		private static readonly Dictionary<string, string> _credentialsDict = new()
		{
			["scope"] = "identify",
			["grant_type"] = "client_credentials"
		};
		
		public DiscordOAuthService(HttpClient client, IOptions<ApiSettings> settings, ILogger<DiscordOAuthService> logger)
		{
			_client = client;
			_logger = logger;
			_settings = settings.Value;
		}

		public async Task<(bool Authenticated, ulong Id)> VerifyDiscordApplicationAsync(string id, string secret)
		{
			// Attempt to generate a bearer token to verify //
			HttpResponseMessage res = CreateBearerToken(id, secret, out string token);

			// Couldn't generate a bearer token //
			if (res.StatusCode is not HttpStatusCode.OK)
				return (false, 0);

			ulong app = await GetApplicationInfoAsync(token);

			// Revoke the token; it's not needed anymore //
			await RevokeBearerTokenAsync(token);

			return (true, app);
		}
		
		private HttpResponseMessage CreateBearerToken(string id, string secret, out string token)
		{
			var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{id}:{secret}"));

			var req = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/v9/oauth2/token")
			{
				Headers = { Authorization = new("Basic", auth) },
				Content = new FormUrlEncodedContent(_credentialsDict)
				{
					Headers =
					{
						ContentType = new("application/x-www-form-urlencoded")
					}
				}
			};

			var res = _client.Send(req);

			using var sr = new StreamReader(res.Content.ReadAsStream(), Encoding.UTF8);

			var response = sr.ReadToEnd();
			
			token = JObject.Parse(response)["access_token"]!.ToString();
			return res;
		}

		private async Task RevokeBearerTokenAsync(string accessToken)
		{
			var content = new KeyValuePair<string, string>("token", accessToken);
			
			var req = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/v9/oauth2/token/revoke")
			{
				Headers =
				{
					Authorization = new("Bearer", accessToken)
				},

				Content = new FormUrlEncodedContent(new[] {content})
				{
					Headers =
					{
						ContentType = new("application/x-www-form-urlencoded")
					}
				}
			};
			
			await _client.SendAsync(req);
		}

		private async Task<ulong> GetApplicationInfoAsync(string accessToken)
		{
			var req = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v9/oauth2/@me")
			{
				Headers =
				{
					Authorization = new("Bearer", accessToken)
				}
			};
			
			var ret = await _client.SendAsync(req);

			var app = (ulong)JObject.Parse(await ret.Content.ReadAsStringAsync()!)["id"];
			return app;
		}
	}
}