using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Extensions;

namespace Silk.Core.Commands.Miscellaneous
{
    [Obsolete("This command is now deprecated, as changelogs will be managed via the Web-Dashboard.")]
    public class ChangelogCommand
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public ChangelogCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [GroupCommand]
        public async Task GetChangeLog(CommandContext ctx)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            if (db.ChangeLogs.Count() is 0)
            {
                await ctx.RespondAsync("It's quite empty here. Perhaps you're using the internal test bot, or this is a self-hosted instance. There are no changelogs to speak of here. :)").ConfigureAwait(false);
                return;
            }

            DiscordEmbed embed = BuildChangeLog(db.ChangeLogs.OrderBy(c => c.ChangeTime).Last());
            await ctx.RespondAsync(embed);
        }

        // [Command("Create")]
        // public async Task CreateChangelog(CommandContext ctx, [RemainingText] string options)
        // {
        //     Changelog changelog = CreateChangelog(options);
        //     var db = new Lazy<SilkDbContext>(() => _dbFactory.CreateDbContext());
        //     DiscordMessage clMessage =
        //         await ctx.RespondAsync("Does this look correct?", BuildChangeLog(changelog));
        //     bool embedAccepted = await CheckConfirmationAsync(ctx, clMessage);
        //     if (embedAccepted)
        //     {
        //         db.Value.ChangeLogs.Add(changelog.ToModel());
        //         await db.Value.SaveChangesAsync();
        //         await clMessage.DeleteAsync();
        //         await clMessage.RespondAsync("Changelog added.");
        //     }
        // }

        // private static async Task<bool> CheckConfirmationAsync(CommandContext context, DiscordMessage message)
        // {
        //     var creationService = context.Services.Get<DiscordEmojiCreationService>();
        //     IEnumerable<DiscordEmoji> emojis = creationService.GetEmoji(":x:", ":white_check_mark:");
        //     DiscordEmoji confirm = emojis.ElementAt(1);
        //     DiscordEmoji deny = emojis.ElementAt(0);
        //     InteractivityExtension interactivity = context.Client.GetInteractivity();
        //     await message.CreateReactionAsync(confirm);
        //     await message.CreateReactionAsync(deny);
        //     InteractivityResult<MessageReactionAddEventArgs> result =
        //         await interactivity.WaitForReactionAsync(m =>
        //             m.Emoji == confirm || m.Emoji == deny && m.User == context.User);
        //     if (result.TimedOut) return false;
        //     return result.Result.Emoji == confirm;
        // }

        private static Changelog CreateChangelog(string cl)
        {
            var delimiter = "|";
            string[] splitOptions = cl.Split(delimiter);
            string additions = splitOptions[0];
            string removals = splitOptions.GetNext();
            string authors = splitOptions.GetNext();
            string version = splitOptions.GetNext();
            DateTime time = DateTime.Now;
            var changelog = new Changelog
                {Additions = additions, Removals = removals, Authors = authors, Version = version, Time = time};
            return changelog;
        }

        private struct Changelog
        {
            public string Additions { get; set; }
            public string Removals { get; set; }
            public string Authors { get; set; }
            public string Version { get; set; }
            public DateTime Time { get; set; }

            public Database.Models.Changelog ToModel()
            {
                return new()
                {
                    Additions = Additions,
                    Authors = Authors,
                    ChangeTime = Time,
                    Removals = Removals,
                    Version = Version
                };
            }
        }

        private static DiscordEmbed BuildChangeLog(Changelog cl)
        {
            var embed = new DiscordEmbedBuilder();
            embed
                .WithTitle($"Changes in version {cl.Version}")
                .WithColor(new DiscordColor("#832fd6"))
                .AddField("Added:", cl.Additions)
                .AddField("Fixed/Removed:", cl.Removals.Length < 1 ? "No information given." : cl.Removals)
                .AddField("Contributers:", $"Changes created by these contributers: {cl.Authors}")
                .WithFooter($"Silk! | Change authored: {cl.Time:MMM d, yyyy}");
            return embed;
        }

        private static DiscordEmbed BuildChangeLog(Database.Models.Changelog cl)
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