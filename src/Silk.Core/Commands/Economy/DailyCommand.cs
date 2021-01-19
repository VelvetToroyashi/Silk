using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class DailyCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;

        public DailyCommand(IDatabaseService dbService) => _dbService = dbService;

        [Command("Daily")]
        [RequireGuild]
        public async Task DailyMoney(CommandContext ctx)
        {
            GlobalUserModel? user = await _dbService.GetGlobalUserAsync(ctx.User.Id);
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);
            
            if (user is null)
            {
                user = new GlobalUserModel {Id = ctx.User.Id, Cash = 500, LastCashOut = DateTime.Now};
                
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(), ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.Green)
                    .WithDescription("I like new faces! Here's a one time bonus of $300 on top of the normal daily!")
                    .WithTitle("Collected $500, come back in 24h for $200 more!");

                
                builder.WithEmbed(embed);
                
                await ctx.RespondAsync(builder);
                await _dbService.UpdateGlobalUserAsync(user);
            }
            else
            {
                if (DateTime.Now.Subtract(user.LastCashOut).TotalDays < 1)
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(), ctx.User.AvatarUrl)
                        .WithColor(DiscordColor.Red)
                        .WithDescription($"You're a little too early! Check back in {user.LastCashOut.AddDays(1).Subtract(DateTime.Now).Humanize(2, minUnit: TimeUnit.Second)}.");
                    builder.WithEmbed(embed);
                    await ctx.RespondAsync(builder);
                }
                else
                {
                    user.Cash += 200;
                    user.LastCashOut = DateTime.Now;
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor(ctx.Member.Nickname, ctx.User.GetUrl(), ctx.User.AvatarUrl)
                        .WithColor(DiscordColor.Green)
                        .WithDescription("Done! I've deposited $200 in your account. Come back tomorrow for more~");
                    builder.WithEmbed(embed);
                    await ctx.RespondAsync(builder);
                    await _dbService.UpdateGlobalUserAsync(user);
                }
            }
        }
    }
}