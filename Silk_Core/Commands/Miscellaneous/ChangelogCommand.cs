using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using SilkBot.Database.Models;
using SilkBot.Extensions;
using SilkBot.Utilities;

namespace SilkBot.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    [Group("changelog")]
    public class ChangelogCommand : CommandClass
    {
        public ChangelogCommand(IDbContextFactory<SilkDbContext> _db) : base(_db) { }

        [GroupCommand]
        public async Task GetChangeLog(CommandContext ctx)
        {
            var db = GetDbContext();
            var embed = BuildChangeLog(db.ChangeLogs.OrderBy(c => c.ChangeTime).Last());
            await ctx.RespondAsync(embed: embed);
        }

        [Command("Create")]
        public async Task CreateChangelog(CommandContext ctx, [RemainingText] string options)
        {
            var changelog = CreateChangelog(options);
            var db = new Lazy<SilkDbContext>(() => GetDbContext());
            var clMessage = await ctx.RespondAsync("Does this look correct?", embed: BuildChangeLog(changelog));
            var embedAccepted = await CheckConfirmationAsync(ctx, clMessage);
            if (embedAccepted)
            {
                db.Value.ChangeLogs.Add(changelog.ToModel());
                await db.Value.SaveChangesAsync();
                await clMessage.DeleteAsync();
                await clMessage.RespondAsync("Changelog added.");
            }
        }

        private async ValueTask<bool> CheckConfirmationAsync(CommandContext context, DiscordMessage message)
        {
            var creationService = context.Services.Get<DiscordEmojiCreationService>();
            IEnumerable<DiscordEmoji> emojis = creationService.GetEmoji(":x:", ":white_check_mark:");
            DiscordEmoji confirm = emojis.ElementAt(1);
            DiscordEmoji deny = emojis.ElementAt(0);
            var interactivity = context.Client.GetInteractivity();
            await message.CreateReactionAsync(confirm);
            await message.CreateReactionAsync(deny);
            var result = (await interactivity.WaitForReactionAsync(m => m.Emoji == confirm || m.Emoji == deny && m.User == context.User));
            if (result.TimedOut) return false;
            return result.Result.Emoji == confirm;
        }
        private Changelog CreateChangelog(string cl)
        {
            var delimiter = "|";
            string[] splitOptions = cl.Split(delimiter);
            string additions = splitOptions[0];
            string removals = (string)splitOptions.GetNext();
            string authors = (string)splitOptions.GetNext();
            string version = (string)splitOptions.GetNext();
            var time = DateTime.Now;
            var changelog = new Changelog { Additions = additions, Removals = removals, Authors = authors, Version = version, Time = time };
            return changelog;
        }

        private struct Changelog
        {
            public string Additions { get; set; }
            public string Removals { get; set; }
            public string Authors { get; set; }
            public string Version { get; set; }
            public DateTime Time { get; set; }
            public ChangelogModel ToModel() => new ChangelogModel
            {
                Additions = Additions,
                Authors = Authors,
                ChangeTime = Time,
                Removals = Removals,
                Version = Version
            };
        }

        private DiscordEmbed BuildChangeLog(Changelog cl)
        {
            var embed = new DiscordEmbedBuilder();
            embed
                .WithTitle($"Changes in version {cl.Version}")
                .WithColor(new DiscordColor("#832fd6"))
                .AddField("Added:", cl.Additions)
                .AddField("Fixed/Removed:", cl.Removals.Count() < 1 ? "No information given." : cl.Removals)
                .AddField("Contributers:", $"Changes created by these contributers: {cl.Authors}")
                .WithFooter($"Silk! | Change authored: {cl.Time:MMM d, yyyy}");
            return embed;
        }
        private DiscordEmbed BuildChangeLog(ChangelogModel cl)
        {
            var embed = new DiscordEmbedBuilder();
            embed
                .WithTitle($"Changes in version {cl.Version}")
                .WithColor(new DiscordColor("#832fd6"))
                .AddField("Added:", cl.Additions)
                .AddField("Removed:", cl.Removals ?? "No deletions in this change.")
                .AddField("Contributers:", $"Changes created by these contributers: {cl.Authors}")
                .WithFooter($"Silk! | Change authored: {cl.ChangeTime:MMM d, yyyy}");
            return embed;
        }
    }
}
