using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Moderation.SClean
{
    public partial class SCleanCommand
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public SCleanCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [Description("Clean images from chat.")]
        public async Task Images(
            CommandContext ctx,
            [Description("How many messages to scan for messages; defaults to 10, limit of 100.")]
            int amount = 10)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();

            amount = amount > 99 ? 100 : ++amount;
            IEnumerable<DiscordMessage> images = await GetImages(ctx.Channel, amount);
            if (!images.Any())
            {
                await ctx.RespondAsync($"Failed to query images in the last {amount} messages.");
                return;
            }

            await ctx.RespondAsync($"Queried {images.Count()} images.");

            await ctx.Channel.DeleteMessagesAsync(images);
        }


        //Discord has said this may be deprecated from the API at some point because the client doesn't use it to render it, but I find it very useful smh.//
        private async Task<IEnumerable<DiscordMessage>> GetImages(DiscordChannel channel, int messageScanCount)
        {
            IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(messageScanCount);
            var @return = new List<DiscordMessage>();
            IEnumerable<DiscordMessage> rawImageMessages = messages.Where(m =>
                m.Attachments.Count > 0 && m.Attachments.Select(a => a.Width).Any(w => w > 0));
            IEnumerable<DiscordMessage> linkImageMessages =
                messages.Where(m => m.Embeds.Select(e => e.Type).Where(t => t == "image").Count() > 0);
            @return.AddRange(rawImageMessages);
            @return.AddRange(linkImageMessages);
            return @return;
        }
    }
}