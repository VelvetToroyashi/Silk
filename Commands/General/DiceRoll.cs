using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using org.mariuszgromada.math.mxparser;
using SilkBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilkBot
{
    public class DiceRoll : BaseCommandModule
    {
        [HelpDescription("Allows you to roll dice!", "`!diceroll d5` → Rolls between 1 and 5.", "`!diceroll 2d4` → Rolls 2 dice, between 1 and 4.", "`!diceroll 2d4+6` → Rolls 2 dice, from 1 to 4, with 6 tacked on to the total.")]
        [Command("DiceRoll")]
        public async Task RollDice(CommandContext ctx, [RemainingText][HelpDescription("The dice to roll.")] string DiceRoll)
        {
            if(DiceRoll is null)
            {
                await ctx.RespondAsync("see `[p]help` diceroll for usage, where `[p]` is the current prefix.");
                return;
            }

            var dieRolls = new List<List<string>>();
            if (DiceRoll[0] == 'd') DiceRoll = "1" + DiceRoll;
            var result = Regex.Replace(DiceRoll, @"([1-9]\d*)d([1-9]\d{0,4})", m =>
            {
                var random = new Random();
                var split = m.Value.ToLower().Split('d');
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
                dieRolls.Add(rolls);



                //Now replace the expressions with their resolutions.

                return totalRoll.ToString();
            });
            var total = new Expression(result).calculate();
            var sb = new StringBuilder();
            foreach(var list in dieRolls)
            {
                sb.AppendLine(string.Join('\n', list));
            }
            await ctx.RespondAsync($"Raw result: \n{sb} Calculated: {total}");
        }



    }
}
