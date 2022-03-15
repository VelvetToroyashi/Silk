using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.Conditions;
using Silk.Extensions;
using Silk.Shared.Configuration;

namespace Silk.Commands.Bot;


public class RestRequestCommand : CommandGroup
{
    private const    string                   baseUri = "https://discord.com/api/v9";
    
    private readonly HttpClient               _client;
    private readonly ICommandContext          _context;
    private readonly IDiscordRestChannelAPI   _channels;
    private readonly SilkConfigurationOptions _silkConfiguration;
    public RestRequestCommand
    (
            HttpClient client,
            ICommandContext context,
            IDiscordRestChannelAPI channels,
            IOptions<SilkConfigurationOptions> silkConfiguration
    )
    {
        _client            = client;
        _context           = context;
        _channels          = channels;
        _silkConfiguration = silkConfiguration.Value;
    }
    
    [Command("rest")]
    [RequireTeamOrOwner]
    [Description("Performs a REST request.")]
    public async Task<Result<IMessage>> DoRestAsync(string method, string uri, [Greedy] string? json = null)
    {
        if (json?.StartsWith("```json") ?? false)
            json = json[8..^3];
        else if (json?.StartsWith("```") ?? false)
            json = json[4..^3];
        
        HttpMethod httpMethod = method.ToUpperInvariant() switch
        {
            "GET"    => HttpMethod.Get,
            "PUT"    => HttpMethod.Put,
            "POST"   => HttpMethod.Post,
            "PATCH"  => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            _        => throw new ArgumentException("Invalid HTTP method.")
        };

        var message = new HttpRequestMessage
        {
            RequestUri = new(baseUri + uri),
            Method     = httpMethod,
            Content    = new StringContent(json ?? "", Encoding.UTF8, "application/json"),
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
                Authorization = new("Bot", _silkConfiguration.Discord.BotToken),
            }
        };
        
        var res = await _client.SendAsync(message);
        var jsn = JToken.Parse(await res.Content.ReadAsStringAsync()).ToString(Formatting.Indented);

        if (jsn.Length <= 1000)
        {
            var embed = new Embed()
            {
                Title = $"{(int)res.StatusCode} - {res.StatusCode.Humanize(LetterCasing.Title)}",
                Colour =
                    (int)res.StatusCode is > 199 and < 400 ? Color.ForestGreen :
                    (int)res.StatusCode is > 399 and < 500 ? Color.DarkOrange : Color.DarkRed,

                Description = "```json\n" + jsn + "\n```"
            };
            
            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] {embed});
        }
        else
        {
            return await _channels.CreateMessageAsync(_context.ChannelID,
                                                      $"{(int)res.StatusCode} - {res.StatusCode.Humanize(LetterCasing.AllCaps)}\n" +
                                                      " The response is too long to Display, so I've sent it as a file~!",
                                                      attachments: new OneOf<FileData, IPartialAttachment>[]
                                                      {
                                                          new FileData("response.json", jsn.AsStream())
                                                      });
        }
    }
}