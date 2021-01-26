using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;
using SkiaSharp;
using Svg.Skia;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class EnlargeCommand : BaseCommandModule
    {
        [Command("enlarge"), Aliases("enbiggen", "emoji", "emote")]
        [Description("Displays a larger version of the provided emoji or custom emote.")]
        public async Task Enlarge(CommandContext ctx, DiscordEmoji emoji)
        {
            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor("36393F"));

            if (emoji.Id != 0) // Guild emote.
            {
                embed.WithFooter(emoji.Name);
                embed.WithImageUrl(emoji.Url);

                await ctx.RespondAsync(embed);
            }
            else // Unicode emote.
            {
                await ctx.TriggerTypingAsync();

                embed.WithFooter(emoji.GetDiscordName().Replace(":", ""));
                embed.WithImageUrl("attachment://emote.jpeg");

                var image = RenderEmoji(emoji.Name);

                var message = new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .WithFile("emote.jpeg", image);

                await message.SendAsync(ctx.Channel);
            }
        }

        private Stream RenderEmoji(string unicodeEmoji)
        {
            Stream svgStream, imageStream = new MemoryStream();

            var emojiHex = char.ConvertToUtf32(unicodeEmoji, 0).ToString("X4");
            var url = $"https://twemoji.maxcdn.com/2/svg/{emojiHex.ToLower()}.svg";
            svgStream = new WebClient().OpenRead(url);

            var svg = new SKSvg();
            svg.Load(svgStream);
            svg.Save(imageStream, SKColor.Empty, scaleX: 4.0f, scaleY: 4.0f);

            imageStream.Position = 0;
            return imageStream;
        }
    }
}