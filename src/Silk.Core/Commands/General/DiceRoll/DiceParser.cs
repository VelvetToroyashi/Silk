using System.Collections.Generic;
using Silk.Shared.Types;

namespace Silk.Core.Commands.General.DiceRoll
{
    internal class DiceParser : TextParser
    {
        public DiceParser(string text) : base(text) { }

        public IList<Step> Run()
        {
            var result = new List<Step>();

            // Read all the dice.
            do
            {
                result.Add(ParseStep());
            } while (ReadIf('+'));

            // Ensure there's no junk data at the end.
            if (ReadChar() != EOT) throw new($"Unexpected character at position {_currentPosition}!");

            return result;
        }

        Step ParseStep()
        {
            // Fill in the quantity as "1" if there's no number and it's a dice.
            if (ReadIf('d')) return ParseDice(1);

            int startNo = ReadNumber();

            if (ReadIf('d')) return ParseDice(startNo);
            return new(StepType.Addition, startNo, 0);
        }

        Step ParseDice(int totalQuantity)
        {
            int noOfSides = ReadNumber();
            return new(StepType.Roll, totalQuantity, noOfSides);
        }
    }
}