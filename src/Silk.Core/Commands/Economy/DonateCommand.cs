using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class DonateCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        private readonly HashSet<ulong> _activeTransactions = new();
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
                return;
            }
            if (sender!.Cash < amount)
            {
                await ctx.RespondAsync($"You're {amount - sender.Cash} dollars too short for that, I'm afraid.");
                return;
            }

            if (amount >= 1000)
            {
                await VerifyTransactionAsync(ctx, sender, receiver!, amount);
            }
            else
            {
                await DoTransactionAsync(ctx, amount, sender, receiver!);
            }


            await _dbService.UpdateGlobalUserAsync(receiver!);
            await _dbService.UpdateGlobalUserAsync(sender!);
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

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (_activeTransactions.Contains(ctx.User.Id))
            {
                await ctx.RespondAsync("You have an active transaction! Complete the first one before donating to someone else!");
                throw new("Donate command does not support concurrent transactions!");
                // I can't think of a better exception to throw; InvalidOp gets typed-matched in the exception handler and prints the help message
                // Which is not ideal.
            }

            _activeTransactions.Add(ctx.User.Id);
        }

        public async override Task AfterExecutionAsync(CommandContext ctx) => _activeTransactions.Remove(ctx.User.Id);

    }
}