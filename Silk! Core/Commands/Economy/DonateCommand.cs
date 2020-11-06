using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using SilkBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilkBot.Commands.Economy
{

    public class DonateCommand : BaseCommandModule
    {

        [Command("Donate")]
        [Aliases("Gift")]
        public async Task Donate(CommandContext ctx, int amount, string recipient)
        {
            var allMembers = await ctx.Guild.GetAllMembersAsync();
            var interactivity = ctx.Client.GetInteractivity();
            var matchingMembers = GetMatchingMembers(allMembers);
            var multipleMatches = matchingMembers.Count() > 1;
            if (matchingMembers.Count() < 1)
            {
                await ctx.RespondAsync("Sorry, I couldn't find anyone matching that name!");
                return;
            }

            if (multipleMatches)
            {
                var matches = new StringBuilder();
                for (var i = 0; i < matchingMembers.Count(); i++)
                {
                    matches.AppendLine($"[{i + 1}]{matchingMembers.ElementAt(i).Mention}");
                }
                
                var embed = EmbedHelper.CreateEmbed(ctx, $"Multiple members matching [{recipient}].", matches.ToString());
                await ctx.RespondAsync(embed: embed);

                var userResposne = await interactivity.WaitForMessageAsync(message => message.Author == ctx.Member && Regex.IsMatch(message.Content, "[1-9]{1,3}"), TimeSpan.FromSeconds(20));
                if (userResposne.TimedOut)
                {
                    await ctx.RespondAsync("Sorry! Your transaction timed out (20 seconds).");
                    return;
                }
                int.TryParse(userResposne.Result.Content, out var selection);
                if (selection > matchingMembers.Count() + 1 || selection == 0)
                {
                    await ctx.RespondAsync("Sorry, but that's not a valid selection");
                    return;
                }
                var finalizedRecipient = matchingMembers.ElementAt((selection < 0 ? 0 : selection) >= matchingMembers.Count() ? matchingMembers.Count() - 1 : selection);
                //TODO: Fix this to work with DB
                //if (!EconomicUsers.Instance.UserExists(ctx.Member.Id))
                //{
                //    CreateEconomicUser(ctx.Member.Id);
                //}

                //if (!EconomicUsers.Instance.UserExists(finalizedRecipient.Id))
                //{
                //    CreateEconomicUser(finalizedRecipient.Id);
                //}

                await ProccessTransaction(ctx, ctx.Member.Id, finalizedRecipient.Id, amount);
            }
            else
            {
                var finalizedRecipient = matchingMembers.First();

                //if (!EconomicUsers.Instance.UserExists(ctx.Member.Id))
                //{
                //    CreateEconomicUser(ctx.Member.Id);
                //}

                //if (!EconomicUsers.Instance.UserExists(finalizedRecipient.Id))
                //{
                //    CreateEconomicUser(finalizedRecipient.Id);
                //}

                await ProccessTransaction(ctx, ctx.Member.Id, finalizedRecipient.Id, amount);
            }

            //TODO: h
            async void CreateEconomicUser(ulong ID)
            {
                //if (!EconomicUsers.Instance.UserExists(ID))
                //{
                //    EconomicUsers.Instance.Add(await ctx.Guild.GetMemberAsync(ID));
                //}
            }

            IEnumerable<DiscordMember> GetMatchingMembers(IEnumerable<DiscordMember> members)
            {
                foreach (var member in members)
                {
                    if (member.IsBot)
                    {
                        continue;
                    }

                    var name = member.DisplayName;
                    var recipientSubstringLength = recipient.Length;
                    if (name.Length < recipient.Length)
                    {
                        recipientSubstringLength = name.Length;
                    }
                    if (name.ToLowerInvariant().Contains(recipient.Substring(0, recipientSubstringLength).ToLowerInvariant()))
                    {
                        yield return member;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        //Note from Lunar: Check usernames, if multiple match, ask user to pick.//

        //TODO: Fix this as well smh
        private async Task ProccessTransaction(CommandContext ctx, ulong sender, ulong receiver, int amount)
        {

        //    var senderAsMember = EconomicUsers.Instance.Users[sender];
        //    var recipientAsMember = EconomicUsers.Instance.Users[receiver];
        //    var rand = new Random();
        //    if (amount > 499)
        //    {
        //        var confirmationCode = rand.Next(1000, 10000);
        //        var interactivity = ctx.Client.GetInteractivity();
        //        await ctx.RespondAsync($"Hey! You sure you want to do this? Confirmation code: `{confirmationCode}`  [Type cancel to cancel]");
        //        while (true)
        //        {
        //            var message = await interactivity.WaitForMessageAsync(message => message.Author == ctx.Member, TimeSpan.FromSeconds(30));
        //            if (message.TimedOut)
        //            {
        //                await ctx.RespondAsync("Sorry! But you did not type the confirmation code. Your transaction has been canceled, and no money was withdrawn.");
        //                continue;
        //            }
        //            if (message.Result.Content != confirmationCode.ToString() && message.Result.Content.ToLower() != "cancel")
        //            {
        //                await ctx.RespondAsync("Invalid or incorrect response code.");
        //                continue;
        //            }
        //            if (message.Result.Content.ToLower() == "cancel")
        //            {
        //                return;
        //            }
        //            if (message.Result.Content == confirmationCode.ToString())
        //            {
        //                if (senderAsMember.Cash < amount)
        //                {
        //                    throw new InsufficientFundsException($"You do not have enough funds for this transaction. [${senderAsMember.Cash} available]");
        //                }
        //                else
        //                {
        //                    senderAsMember.Widthdraw((uint)amount);
        //                    recipientAsMember.Cash += (uint)amount;

        //                    await ctx.RespondAsync(embed: new
        //                    DiscordEmbedBuilder()
        //                    .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
        //                    .WithTitle("Transfer Successful!")
        //                    .WithDescription($"You sent {(await ctx.Guild.GetMemberAsync(receiver)).Mention} ${amount}!")
        //                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
        //                    .WithColor(DiscordColor.Green)
        //                    .WithTimestamp(DateTime.Now)
        //                    );
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        senderAsMember.Widthdraw((uint)amount);
        //        recipientAsMember.Cash += (uint)amount;

        //        await ctx.RespondAsync(embed: new
        //        DiscordEmbedBuilder()
        //        .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
        //        .WithTitle("Transfer Successful!")
        //        .WithDescription($"You sent {(await ctx.Guild.GetMemberAsync(receiver)).Mention} ${amount}!")
        //        .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
        //        .WithColor(DiscordColor.Green)
        //        .WithTimestamp(DateTime.Now));
        //    }
        }
    }
}
