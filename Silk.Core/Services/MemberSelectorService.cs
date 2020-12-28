#pragma warning disable CA1822 // Mark members as static

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using SilkBot.Extensions;

namespace Silk.Core.Services
{
    public class MemberSelectorService
    {
        public async Task<DiscordUser> SelectUser(CommandContext ctx, IEnumerable<DiscordUser> users)
        {
            DiscordEmbedBuilder selectorEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue)
                                                                         .WithTitle(
                                                                             "There are multiple people matching that name; which one do you want?")
                                                                         .AddFooter(ctx);
            var userString = string.Empty;
            for (var i = 0; i < System.Math.Min(10, users.Count()); i++)
                userString += $"{i}: {users.ElementAt(i).Mention}\n";
            selectorEmbed.WithDescription(userString);
            await ctx.RespondAsync(embed: selectorEmbed);
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> result;
            while (true)
            {
                result = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User);
                if (result.TimedOut) break;
                if (result.Result.Content.ToLower() != "cancel")
                {
                    if (!Regex.IsMatch(result.Result.Content, @"^[0-9]+$"))
                    {
                        await ctx.RespondAsync("That's not a valid selection.");
                    }
                    else
                    {
                        int index = int.Parse(result.Result.Content);
                        if (index > users.Count())
                        {
                            await ctx.RespondAsync("That's not a valid selection.");
                        }
                        else
                        {
                            return users.ElementAt(index);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return null;
        }
    }
}