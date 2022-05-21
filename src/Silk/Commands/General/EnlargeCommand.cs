using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Utilities.HelpFormatter;
using SkiaSharp;
using Svg.Skia;

namespace Silk.Commands.General;


[Category(Categories.General)]
public class EnlargeCommand : CommandGroup
{
    
    private readonly HttpClient             _http;
    private readonly ICommandContext       _context;
    private readonly IDiscordRestChannelAPI _channels;
    public EnlargeCommand
    (
        HttpClient http,
        ICommandContext context,
        IDiscordRestChannelAPI channels
    )
    {
        _http     = http;
        _context  = context;
        _channels = channels;
    }

    [Command("enlarge", "enbiggen", "emoji", "emote")]
    [Description("Displays a larger version of the provided emoji or custom emote.")]
    public async Task<Result<IMessage>> Enlarge(IPartialEmoji emoji)
    {
        if (emoji.ID.IsDefined()) // Guild emote.
        {
            if (!emoji.ID.IsDefined(out var emojiID))
                return Result<IMessage>.FromError(new ArgumentInvalidError(nameof(emoji), "The provided emoji is not a valid emote."));
            
            var emojiUriResult = CDN.GetEmojiUrl(emojiID.Value, imageSize: 256);

            if (!emojiUriResult.IsDefined(out var emojiURI))
                return Result<IMessage>.FromError(emojiUriResult.Error!);

            var regexedEmojiURL = Regex.Match(emojiURI.ToString(), @"^https://cdn\.discordapp\.com/emojis/(?<EMOJI>\d+)\.(?<EXT>gif|png)(?:\?size\=\d+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
            var emojiName = regexedEmojiURL.Groups["EMOJI"].Value + '.' + regexedEmojiURL.Groups["EXT"].Value;
            
            return await _channels.CreateMessageAsync
                (
                 _context.ChannelID,
                 attachments: new OneOf<FileData, IPartialAttachment>[]
                 {
                     new FileData(emojiName, await _http.GetStreamAsync(emojiURI), null!)
                 });
        }
        else // Unicode emote.1
        {
            if (!emoji.Name.IsDefined(out var name))
                return Result<IMessage>.FromError(new ArgumentInvalidError(nameof(emoji), "The provided emoji is not a valid emote."));
            
            return await _channels.CreateMessageAsync
                (
                 _context.ChannelID,
                 attachments: new OneOf<FileData, IPartialAttachment>[]
                 {
                     new FileData(name + ".png", await RenderEmojiAsync(name), null!)
                 }
                );
        }
        
        return Result<IMessage>.FromError(new InvalidOperationError("Unable to find an emoji to enlarge."));
    }

    private async Task<Stream> RenderEmojiAsync(string unicodeEmoji)
    {
        Stream svgStream,
               imageStream = new MemoryStream();

        var emojiHex = char.ConvertToUtf32(unicodeEmoji, 0).ToString("X4");
        var url      = $"https://twemoji.maxcdn.com/2/svg/{emojiHex.ToLower()}.svg";
        svgStream = await _http.GetStreamAsync(url);

        var svg = new SKSvg();
        svg.Load(svgStream);
        svg.Save(imageStream, SKColor.Empty, scaleX: 7.13f, scaleY: 7.13f);

        imageStream.Position = 0;
        return imageStream;
    }
}