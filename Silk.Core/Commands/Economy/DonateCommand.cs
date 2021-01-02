using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class DonateCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public DonateCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command("Donate")]
        [Aliases("Gift")]
        public async Task Donate(CommandContext ctx, uint amount, DiscordMember recipient)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            GlobalUserModel? sender = db.GlobalUsers.FirstOrDefault(u => u.Id == ctx.User.Id);
            GlobalUserModel? receiver = db.GlobalUsers.FirstOrDefault(u => u.Id == recipient.Id);


            if (sender is null && receiver is null)
            {
                await ctx.RespondAsync(
                    $"Hmm. Seems like neither of you have an account here. Go ahead and do `{ctx.Prefix}daily` for me and I'll give you some cash to send to your friend *:)*");
                return;
            }

            if (receiver is null)
            {
                receiver = new GlobalUserModel {Id = recipient.Id};
                db.GlobalUsers.Add(receiver);
            }

            if (receiver == sender)
                await ctx.RespondAsync(
                    "I'd love to duplicate money just as much as the next person, but we have an economy!");
            else if (sender.Cash < amount)
                await ctx.RespondAsync($"You're {amount - sender.Cash} dollars too short for that, I'm afraid.");
            else if (amount >= 1000) await VerifyTransactionAsync(ctx, sender, receiver, amount);
            else await DoTransactionAsync(ctx, amount, sender, receiver);

            await db.SaveChangesAsync();
        }

        private async Task DoTransactionAsync(CommandContext ctx, uint amount, GlobalUserModel sender,
            GlobalUserModel receiver)
        {
            DiscordMember member = await ctx.Guild.GetMemberAsync(receiver.Id);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                        .WithThumbnail(ctx.User.AvatarUrl)
                                        .WithDescription(
                                            $"Successfully donated {amount} dollars to {member.Mention}! Feel free to do `{ctx.Prefix}cash` to ensure you've received the funds.")
                                        .WithColor(DiscordColor.PhthaloGreen);
            sender.Cash -= (int)amount;
            receiver.Cash += (int)amount;
            await ctx.RespondAsync(embed: embed);
        }

        private async Task VerifyTransactionAsync(CommandContext ctx, GlobalUserModel sender, GlobalUserModel receiver,
            uint amount)
        {
            // 'Complicated async logic here' //
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            int authKey = new Random().Next(1000, 10000);
            await ctx.RespondAsync(
                $"Just verifying you want to send money to this person. Could you type `{authKey}` to confirm? (Ignoring this will cancel, since Velvet can't be bothered to write that logic right now.)");
            InteractivityResult<DiscordMessage> message =
                await interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.Content == authKey.ToString(),
                    TimeSpan.FromMinutes(3));
            if (message.TimedOut)
            {
                await ctx.RespondAsync("Timed out :(");
            }
            else
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync(receiver.Id);
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                            .WithThumbnail(ctx.User.AvatarUrl)
                                            .WithDescription(
                                                $"Successfully donated {amount} dollars to {member.Mention}! Feel free to do `{ctx.Prefix}cash` to ensure you've received the funds.")
                                            .WithColor(DiscordColor.PhthaloGreen);
                sender.Cash -= (int)amount;
                receiver.Cash += (int)amount;
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}