using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using org.mariuszgromada.math.mxparser;
using SilkBot.Extensions;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class DiceRoll : BaseCommandModule
    {
        [Command]
        public async Task Random(CommandContext ctx, int max = 100) =>
            await ctx.RespondAsync(new Random().Next(max).ToString()).ConfigureAwait(false);


        private enum State
        {
            DiceMult,
            DiceSign,
            DiceSide,
            Unknown,
            DiceAddi,
            Modifier
        }
        
        
        //I'll get back to this later :c
        [Command]
        public async Task Roll(CommandContext ctx, [RemainingText] string roll)
        {
            // if (roll.Any(r => r is >= '0' and <= '9' and not 'd' or '+' or ' '))
            //     throw new ArgumentException("That doesn't seem to be a valid roll..");
            
            State lastState = State.Unknown;

            List<State> stateList = new();
            
            for (int i = 0; i < roll.Length; i++)
            {
                lastState = roll[i] switch
                {
                    '+'                                               => State.DiceAddi,
                    'd'                                               => State.DiceSign,
                    >= '1' or <= '0' when lastState is State.DiceAddi => State.Modifier,
                    >= '1' or <= '0' when lastState is State.Unknown  => State.DiceMult,
                    >= '1' or <= '0' when lastState is State.DiceSide => State.DiceSide,
                    >= '1' or <= '0' when lastState is State.DiceSign => State.DiceSide,
                    >= '1' or <= '0' when lastState is State.DiceMult => State.DiceMult,
                    _                                                 => State.Unknown
                };
                stateList.Add(lastState);
            }

            await ctx.RespondAsync(
                         stateList
                             .GroupBy(i => i)
                             .Select(s => $"{s.Key} ({s.Count()} times)")
                             .JoinString("\n"))
                             .ConfigureAwait(false);

        }
    }
}