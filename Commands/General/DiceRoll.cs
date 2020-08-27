using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using org.mariuszgromada.math.mxparser;
using SilkBot.Exceptions;
using System;
using System.Linq;
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
            if (DiceRoll[0] == 'd') DiceRoll.ToCharArray().Reverse().Append('1').Reverse();
            var result = Regex.Replace(DiceRoll, @"([1-9]\d*)d([1-9]\d{0,4})", m =>
            {
                var r = new Random();
                var split = m.Value.ToLower().Split('d');
                var dieCount = int.Parse(split[0]);
                var dieSize = int.Parse(split[1]);

                var totalRoll = 0;

                for (var i = 0; i < dieCount; i++)
                {


                    totalRoll += r.Next(1, dieSize + 1);
                }




                //Now replace the expressions with their resolutions.

                return totalRoll.ToString();
            });
            var total = new Expression(result).calculate();
            await ctx.RespondAsync($"Raw result: {result}, Calculated: {total}");
        }



    }
}
