using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SilkBot.Economy;
using System;
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
        public async Task Donate(CommandContext ctx, int Amount, DiscordMember Recipient)
        {

            var members = await ctx.Guild.GetAllMembersAsync();
            var interactivity = ctx.Client.GetInteractivity();

            if(members.Where(member => member.DisplayName.Substring(0, 2).Contains(Recipient.DisplayName.Substring(0, 2))).Count() > 1)
            {
                var matches = members.Where(member => member.DisplayName.ToLower().Substring(0, 3).Contains(Recipient.DisplayName.Substring(0, 3).ToLower())).ToList();
                var embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, ctx.User.AvatarUrl)
                    .WithTitle("I found multiple people matching that username:")
                    .WithColor(DiscordColor.Blue);
                var matchesSb = new StringBuilder();
                for(int i = 0; i < matches.Count(); i++)
                {
                    matchesSb.AppendLine($"{i + 1}: {matches[i].Mention}");
                }
                embed.WithDescription(matchesSb.ToString() ?? "Embed broke b");

                await ctx.RespondAsync(embed: embed);

                var message = await interactivity.WaitForMessageAsync(message => message.Author.Id == ctx.User.Id && Regex.Match(message.Content, "[1-9]?[1-9]").Success, TimeSpan.FromSeconds(10));

                if(message.Result.Content is null)
                {
                    await ctx.RespondAsync("Transaction timed out.");
                    return;
                }
                else
                {

                }
                
                return;
            }

            else 
            {
                if (!EconomicUsers.Instance.UserExists(ctx.Member.Id))
                {
                    throw new Exception("Sorry, but you haven't setup an account with Silk! `!daily` will set you up.");
                }
                else
                {
                    if (!EconomicUsers.Instance.UserExists(Recipient.Id))
                    {
                        EconomicUsers.Instance.Add(ctx, Recipient);
                    }

                    var Sender = EconomicUsers.Instance.Users[ctx.Member.Id];
                    var Receiver = EconomicUsers.Instance.Users[Recipient.Id];

                    if (Sender.Cash < Amount)
                        throw new InsufficientFundsException("You do not have enough funds to send this person!");

                    Sender.Widthdraw((uint)Amount);
                    Receiver.Cash += (uint)Amount;

                    await ctx.RespondAsync(embed: new
                        DiscordEmbedBuilder()
                        .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                        .WithTitle("Transfer Successful!")
                        .WithDescription($"You sent {Recipient.Mention} {Amount} coins!")
                        .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                        .WithColor(DiscordColor.Green)
                        .WithTimestamp(DateTime.Now)
                        );
                }
            }
            
     


        }


        //Note from Lunar: Check usernames, if multiple match, ask user to pick.//

    }
}
