using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using org.mariuszgromada.math.mxparser;
using SilkBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilkBot
{
    public class DiceRoll : BaseCommandModule
    {
        [HelpDescription("Allows you to roll dice!", "`!diceroll d5` → Rolls between 1 and 5.", "`!diceroll 2d4` → Rolls 2 dice, between 1 and 4.", "`!diceroll 2d4+6` → Rolls 2 dice, from 1 to 4, with 6 tacked on to the total." )]
        [Command("DiceRoll")]
        public async Task RollDice(CommandContext ctx, [RemainingText] [HelpDescription("The dice to roll.")] string diceRoll)
        {
            if (!diceRoll.ToLower().Contains('d'))
                throw new InvalidDiceRollException("I couldn't tell what dice you're trying to roll!");

            if (diceRoll[0] == 'd')            
                diceRoll = "1" + diceRoll;
              
                

            var matcher = Regex.Replace(diceRoll, "[1-9]?[0-9](d|D)[1-9]?[0-9]", m =>
            {
                var r = new Random();
                var split = m.Value.ToLower().Split('d');
                var dieCount = int.Parse(split[0]);
                var dieSize = int.Parse(split[1]);

                var totalRoll = 0;

                    for (var i = 0; i < dieCount; i++)
                    {
                    //Get random roll between 1
                    //and the die size.
                    
                        totalRoll += r.Next(1, dieSize + 1);
                    }
                    
            


                //Now replace the expressions with their resolutions.
                
                return totalRoll.ToString();
            });

            
            
            int total = (int)new Expression(matcher).calculate();
            if (total == int.MaxValue || total == int.MinValue)
            {
                await ctx.RespondAsync("Dice roll resulted in overflow! (Your diceroll was too great for computational power.");
                return;
            }
            await ctx.RespondAsync($"You rolled {total}");
        }




    }
}
