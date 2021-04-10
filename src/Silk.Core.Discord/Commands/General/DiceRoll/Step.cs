namespace Silk.Core.Discord.Commands.General.DiceRoll
{
    internal enum StepType
    {
        Roll,
        Addition
    }

    internal struct Step
    {
        public StepType Type;

        // The quantity of this dice or the number to add.
        public int TotalNumber;
        public int DiceNoSides;

        public Step(StepType type, int totalNo, int diceNoSides)
        {
            (Type, TotalNumber, DiceNoSides) = (type, totalNo, diceNoSides);
        }
    }
}