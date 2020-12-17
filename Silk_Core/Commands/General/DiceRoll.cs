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
        struct Dice
        {
            public int TotalNumber;
            public int NoSides;
        }

        [Command]
        public async Task Random(CommandContext ctx, int max = 100) =>
            await ctx.RespondAsync(new Random().Next(max).ToString()).ConfigureAwait(false);

        [Command]
        public async Task Roll(CommandContext ctx, [RemainingText] string roll)
        {
            var parser = new DiceParser(roll);
            var allDice = parser.Run();

            // Test printing:
            var sb = new StringBuilder();
            for (int i = 0; i < allDice.Count; i++)
            {
                sb.Append(allDice[i].TotalNumber);
                sb.Append(" x ");
                sb.Append(allDice[i].NoSides);
                sb.Append(" sides");

                if (i < allDice.Count - 1) sb.Append(", ");
            }

            await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
        }

        class DiceParser : TextParser
        {
            public DiceParser(string text) : base(text) { }

            public IList<Dice> Run()
            {
                var result = new List<Dice>();

                // Read all the dice.
                do result.Add(ParseDice());
                while (ReadIf('+'));

                // Ensure there's no junk data at the end.
                if (Read() != EOT) throw new Exception($"Unexpected character {CurrentPosition}");

                return result;
            }

            public Dice ParseDice()
            {
                var res = new Dice();

                res.TotalNumber = ReadNumberOr1();

                // Make sure there's definitely a "d" in the middle.
                if (!ReadIf('d')) throw new Exception($"Unexpected character at position {CurrentPosition}!");

                res.NoSides = ReadNumberOr1();

                return res;
            }
        }
    }
}