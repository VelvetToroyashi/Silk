using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.Extensions;
using Silk.Shared.Configuration;

namespace Silk.Core.Commands.Bot
{
    [Hidden]
    public class RestRequestCommand : BaseCommandModule
    {
        private const string baseUri = "https://discord.com/api/v9";
        private readonly IHttpClientFactory _clientFactory;
        private readonly SilkConfigurationOptions _silkConfiguration;

        public RestRequestCommand(IHttpClientFactory clientFactory, IOptions<SilkConfigurationOptions> options)
        {
            _clientFactory = clientFactory;
            _silkConfiguration = options.Value;
        }

        [Command]
        public async Task Rest(CommandContext ctx, string method, string uri, [RemainingText] string? json = null)
        {
            if (json?.StartsWith("```json") ?? false)
                json = json[8..^3];
            else if (json?.StartsWith("```") ?? false)
                json = json[4..^3];

            HttpClient? client = _clientFactory.CreateClient();
            HttpMethod? httpMethod = method.ToUpperInvariant() switch
            {
                "GET" => HttpMethod.Get,
                "PUT" => HttpMethod.Put,
                "POST" => HttpMethod.Post,
                "PATCH" => HttpMethod.Patch,
                "DELETE" => HttpMethod.Delete,
                _ => throw new ArgumentException("Invalid HTTP method.")
            };

            var message = new HttpRequestMessage
            {
                RequestUri = new(baseUri + uri),
                Method = httpMethod,
                Content = new StringContent(json ?? "", Encoding.UTF8, "application/json"),
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bot {_silkConfiguration.Discord.BotToken}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage? res = await client.SendAsync(message);
            var jsn = JToken.Parse(await res.Content.ReadAsStringAsync() ?? "{}").ToString(Formatting.Indented);

            if (jsn.Length > 1000)
            {
                await new DiscordMessageBuilder()
                    .WithContent($"{(int)res.StatusCode} - {res.StatusCode.Humanize(LetterCasing.AllCaps)}\n(Response was > 1000 characters. I've attached the output to a file for you)")
                    .WithFile("response.json", jsn.AsStream())
                    .SendAsync(ctx.Channel);
            }
            else
            {
                await new DiscordMessageBuilder()
                    .WithContent($"{(int)res.StatusCode} - {res.StatusCode.Humanize(LetterCasing.AllCaps)}\n {(jsn == "{}" ? "Request returned empty response." : Formatter.BlockCode(jsn, "json"))}")
                    .SendAsync(ctx.Channel);
            }
        }
    }
}