using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.SClean
{
    public partial class SCleanCommand
    {
        [Command]
        public async Task Images(CommandContext ctx, [HelpDescription("The amount of images you want to clean from chat; limit 100 per call")] int amount = 10)
        {
            amount = amount > 100 ? amount : 100;
            var images = await GetImages(ctx.Channel, amount);
            await ctx.RespondAsync($"Queried {images.Count} images.");
            await ctx.Channel.DeleteMessagesAsync(images);
        }

        private async Task<ICollection<DiscordMessage>> GetImages(DiscordChannel channel, int requsiteAmount)
        {
            var firstPass = true;
            var images = new List<DiscordMessage>();
            ulong lastMessage = 0;
            while(images.Count < requsiteAmount)
            {
                if (firstPass)
                {
                    firstPass = false;
                    DiscordMessage[] messages = (await channel.GetMessagesAsync(100)).ToArray();
                    DiscordMessage[] firstPassImages = messages.Where(m => m.Attachments.Count > 0 && m.Attachments.First().Width != 0).ToArray();
                    images.AddRange(firstPassImages.Length > requsiteAmount ? firstPassImages.Take(requsiteAmount) : firstPassImages.AsEnumerable());
                    lastMessage = messages.OrderBy(m => m.CreationTimestamp).Last().Id;
                }
                DiscordMessage[] msgs = (await channel.GetMessagesBeforeAsync(lastMessage, 100)).ToArray();
                lastMessage = msgs.OrderBy(m => m.CreationTimestamp).Last().Id;
                DiscordMessage[] imgs = msgs.Where(m => m.Attachments.Count > 0 && m.Attachments.First().Width != 0).ToArray();
                images.AddRange(imgs.Length > requsiteAmount - images.Count ? imgs.Take(requsiteAmount - images.Count) : images.AsEnumerable());
            }
            return images;
        }
    }
}
