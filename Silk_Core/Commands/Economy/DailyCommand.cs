using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using SilkBot.Database.Models;
using SilkBot.Extensions;
using SilkBot.Utilities;


namespace SilkBot.Commands.Economy
{
    [Category(Categories.Economy)]
    public class DailyCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public DailyCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command("Daily")]
        [RequireGuild()]
        public async Task DailyMoney(CommandContext ctx)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            GlobalUserModel user = db.GlobalUsers.FirstOrDefault(u => u.Id == ctx.User.Id);
            if (user is null)
            {
                user = new GlobalUserModel {Id = ctx.User.Id, Cash = 500, LastCashOut = DateTime.Now};
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                            .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(), ctx.Member.AvatarUrl)
                                            .WithColor(DiscordColor.Green)
                                            .WithDescription(
                                                "It appears this is your first time I've seen you. I'm feeling extra generous, and have given you an extra three hundred dollars" +
                                                " on top of normal daily rates *:)* Don't spend it all in one place~")
                                            .WithTitle("Collected $500, come back in 24h for $200 more!");
                await ctx.RespondAsync(embed: embed);
                db.GlobalUsers.Add(user);
                await db.SaveChangesAsync();
            }
            else
            {
                if (DateTime.Now.Subtract(user.LastCashOut).TotalDays < 1)
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                                .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(),
                                                    ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Red)
                                                .WithDescription(
                                                    $"You're a little too early! Check back in {user.LastCashOut.AddDays(1).Subtract(DateTime.Now).Humanize(2, minUnit: TimeUnit.Second)}.");
                    await ctx.RespondAsync(embed: embed);
                }
                else
                {
                    user.Cash += 200;
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                                .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(),
                                                    ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Green)
                                                .WithDescription(
                                                    $"Done! I've deposited $200 in your account. Come back tomorrow for more~");
                    await ctx.RespondAsync(embed: embed);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}