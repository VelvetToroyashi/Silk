using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using org.mariuszgromada.math.mxparser;
using SilkBot.Extensions;
using SilkBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class DiceRoll : BaseCommandModule
    {

        [Command("DiceRoll")]
        [HelpDescription("Allows you to roll dice!", "`!diceroll d5` → Rolls between 1 and 5.", "`!diceroll 2d4` → Rolls 2 dice, between 1 and 4.", "`!diceroll 2d4+6` → Rolls 2 dice, from 1 to 4, with 6 tacked on to the total.")]
        public async Task RollDice(CommandContext ctx, [RemainingText, HelpDescription("The dice to roll.")] string DiceRoll)
        {
            if (DiceRoll is null)
            {
                await ctx.RespondAsync("see `[p]help` diceroll for usage, where `[p]` is the current prefix.");
                return;
            }

            string changed = Regex.Replace(DiceRoll, @"([1-9]*)d([1-9]{0,4})", string.Empty);
            changed = string.Join(", ", Regex.Matches(changed, @"([0-9])+"));

            (var dieRolls, var result) = CalculateDiceValues(DiceRoll);
            double total = new Expression(result).calculate();
            var sb = new StringBuilder();

            sb.Append(GetFormattedRollsString(dieRolls));
            sb.Append($"*Modifiers applied: {(changed == "" ? "none" : changed)}* \n*Total: {total}*");
            var embed = EmbedHelper.CreateEmbed(ctx, sb.ToString(), DiscordColor.Blurple);
            await ctx.RespondAsync(embed: embed);
        }

        public (List<List<string>> rollList, string rollString) CalculateDiceValues(string diceRollString)
        {
            var rollList = new List<List<string>>();
            var rollString = Regex.Replace(diceRollString, @"([1-9]*)d([1-9]{0,4})", m =>
            {
                var random = new Random();
                string match = m.Value;
                if (m.Value[0] == 'd')
                {
                    match = new string(match.Prepend('1').ToArray());
                }

                var split = match.ToLower().Split('d');
                var dieCount = int.Parse(split[0]);
                var dieSize = int.Parse(split[1]);

                var totalRoll = 0;
                var rolls = new List<string>();
                for (var i = 0; i < dieCount; i++)
                {
                    var nextRoll = random.Next(1, dieSize + 1);
                    totalRoll += nextRoll;
                    rolls.Add($"Die {i + 1}: {nextRoll}");
                }
                rollList.Add(rolls);
                //Now replace the expressions with their resolutions
                return totalRoll.ToString();
            }, RegexOptions.Compiled);
            return (rollList, rollString);
        }
        public string GetFormattedRollsString(List<List<string>> rolls)
        {
            var sb = new StringBuilder();

            foreach (var rollList in rolls)
            {
                foreach (var roll in rollList)
                {
                    sb.AppendLine($":game_die: {roll}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }
}
