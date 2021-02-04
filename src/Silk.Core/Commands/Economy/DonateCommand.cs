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
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class DonateCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;

        public DonateCommand(IDatabaseService dbService)
        {
            _dbService = dbService;
        }

        [Command("donate")]
        [Aliases("gift")]
        [Description("Send a Guild member some sweet cash!")]
        public async Task Donate(CommandContext ctx, uint amount, DiscordMember recipient)
        {
            GlobalUser sender = await _dbService.GetOrCreateGlobalUserAsync(ctx.User.Id);
            GlobalUser receiver = await _dbService.GetOrCreateGlobalUserAsync(recipient.Id);

            if (receiver == sender)
            {
                await ctx.RespondAsync("I'd love to duplicate money just as much as the next person, but we have an economy!");
            }
            else if (sender!.Cash < amount)
            {
                await ctx.RespondAsync($"You're {amount - sender.Cash} dollars too short for that, I'm afraid.");

            }
            else if (amount >= 1000)
            {
                await VerifyTransactionAsync(ctx, sender, receiver!, amount);

            }
            else
            {
                await DoTransactionAsync(ctx, amount, sender, receiver!);
                await _dbService.UpdateGlobalUserAsync(receiver!);
                await _dbService.UpdateGlobalUserAsync(sender!);
            }
        }
        
        private static async Task DoTransactionAsync(CommandContext ctx, uint amount, GlobalUser sender, GlobalUser receiver)
        {
            // We use uint as an easier way of anti fraud protection; people would put a negative number and essentially steal money from others. //
            DiscordMember member = await ctx.Guild.GetMemberAsync(receiver.Id);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithThumbnail(ctx.User.AvatarUrl)
                .WithDescription($"Successfully donated {amount} dollars to {member.Mention}! " +
                                 $"Feel free to do `{ctx.Prefix}cash` to ensure you've received the funds.")
                .WithColor(DiscordColor.PhthaloGreen);

            sender.Cash -= (int) amount;
            receiver.Cash += (int) amount;

            await ctx.RespondAsync(embed);
        }

        private static async Task VerifyTransactionAsync(CommandContext ctx, GlobalUser sender, GlobalUser receiver, uint amount)
        {
            // 'Complicated async logic here' //
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            int authKey = new Random().Next(1000, 10000);
            await ctx.RespondAsync("Just verifying you want to send money to this person. " +
                                   $"Could you type `{authKey}` to confirm? (Ignoring this will cancel!)");
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
                    .WithDescription($"Successfully donated {amount} dollars to {member.Mention}! " +
                                     $"Feel free to do `{ctx.Prefix}cash` to ensure you've received the funds.")
                    .WithColor(DiscordColor.PhthaloGreen);
                
                sender.Cash -= (int) amount;
                receiver.Cash += (int) amount;
                
                await ctx.RespondAsync(embed);
            }
        }
    }
}