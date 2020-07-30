using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SilkBot.Commands.GeneralCommands
{
    public class Enbiggen : BaseCommandModule
    {
        [Command("Enlarge")]
        public async Task Enlarge(CommandContext ctx, DiscordEmoji emoji)
        {
            var ranFileName = new Random().Next();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Emojis");
            var ext = emoji.Url.Substring(emoji.Url.Length - 3);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(emoji.Url), Path.Combine(path, $"{ranFileName}.{ext}"));
            }
            await ctx.RespondWithFileAsync(Path.Combine(path, $"{ranFileName}.{ext}"));        
            
        }
    }
}
