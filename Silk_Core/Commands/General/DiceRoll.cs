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
        enum StepType
        {
            Roll, Addition
        }
        
        struct Step
        {
            public StepType Type;

            // The quantity of this dice or the number to add.
            public int TotalNumber;
            public int DiceNoSides;

            public Step(StepType type, int totalNo, int diceNoSides) => (Type, TotalNumber, DiceNoSides) = (type, totalNo, diceNoSides);
        }

        [Command]
        public async Task Random(CommandContext ctx, int max = 100) =>
            await ctx.RespondAsync(new Random().Next(max).ToString()).ConfigureAwait(false);

        [Command]
        public async Task Roll(CommandContext ctx, [RemainingText] string roll)
        {
            var parser = new DiceParser(roll);
            var steps = parser.Run();

            // TEST CODE
            var sb = new StringBuilder();
            for (int i = 0; i < steps.Count; i++)
            {
                sb.Append(steps[i].Type.ToString());

                sb.Append('(');
                sb.Append(steps[i].TotalNumber);

                if (steps[i].Type == StepType.Roll)
                {
                    sb.Append(" x ");
                    sb.Append(steps[i].DiceNoSides);
                    sb.Append(" sides");
                }

                sb.Append(')');

                if (i < steps.Count - 1) sb.Append(", ");
            }

            await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
        }

        class DiceParser : TextParser
        {
            public DiceParser(string text) : base(text) { }

            public IList<Step> Run()
            {
                var result = new List<Step>();

                // Read all the dice.
                do
                {
                    result.Add(ParseStep());
                }
                while (ReadIf('+'));

                // Ensure there's no junk data at the end.
                if (Read() != EOT) throw new Exception($"Unexpected character at position {CurrentPosition}!");

                return result;
            }

            Step ParseStep()
            {
                // Fill in the quantity as "1" if there's no number and it's a dice.
                if (ReadIf('d')) return ParseDice(1);

                var startNo = ReadNumber();

                if (ReadIf('d')) return ParseDice(startNo);
                else return new Step(StepType.Addition, startNo, 0);
            }

            Step ParseDice(int totalQuantity)
            {
                var noOfSides = ReadNumber();
                return new Step(StepType.Roll, totalQuantity, noOfSides);
            }
        }
    }
}