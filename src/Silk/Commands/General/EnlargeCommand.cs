using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Utilities.HttpClient;
using Silk.Utilities.HelpFormatter;
using SkiaSharp;
using Svg.Skia;

namespace Silk.Commands.General;

[HelpCategory(Categories.General)]
public class EnlargeCommand : BaseCommandModule
{
    private readonly IHttpClientFactory _httpClientFactory;
    public EnlargeCommand(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [Command("enlarge")] [Aliases("enbiggen", "emoji", "emote")]
    [Description("Displays a larger version of the provided emoji or custom emote.")]
    public async Task Enlarge(CommandContext ctx, DiscordEmoji emoji)
    {

        DiscordEmbedBuilder? embed = new DiscordEmbedBuilder().WithColor(new("2F3136"));
        if (emoji.Id != 0) // Guild emote.
        {
            var message = new DiscordMessageBuilder();
            embed.WithFooter(emoji.Name);
            embed.WithImageUrl(emoji.Url + "?size=2048");
            message.WithEmbed(embed)
                   .WithReply(ctx.Message.Id);
            await ctx.RespondAsync(message);
        }
        else // Unicode emote.
        {
            await ctx.TriggerTypingAsync();

            embed.WithFooter(Regex.Replace(emoji.GetDiscordName(), "(?<emote>:[A-z_-0-9])", "(?<emote>)"));
            embed.WithImageUrl("attachment://emote.jpeg");

            Stream? image = await RenderEmojiAsync(emoji.Name);

            DiscordMessageBuilder? message = new DiscordMessageBuilder()
                                            .WithEmbed(embed)
                                            .WithFile("emote.jpeg", image)
                                            .WithReply(ctx.Message.Id);

            await message.SendAsync(ctx.Channel);
        }
    }

    private async Task<Stream> RenderEmojiAsync(string unicodeEmoji)
    {
        Stream svgStream,
               imageStream = new MemoryStream();

        var emojiHex = char.ConvertToUtf32(unicodeEmoji, 0).ToString("X4");
        var url      = $"https://twemoji.maxcdn.com/2/svg/{emojiHex.ToLower()}.svg";
        svgStream = await _httpClientFactory.CreateSilkClient().GetStreamAsync(url);

        var svg = new SKSvg();
        svg.Load(svgStream);
        svg.Save(imageStream, SKColor.Empty, scaleX: 16.0f, scaleY: 16.0f);

        imageStream.Position = 0;
        return imageStream;
    }
}