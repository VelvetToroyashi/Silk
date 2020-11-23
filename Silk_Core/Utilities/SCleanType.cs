#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0414 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace SilkBot.Utilities
{
    public class SCleanTypeParser : IArgumentConverter<SCleanType>
    {
        private enum State
        {
            None,
            OnFlag,
            Busy,
            Errored
        }

        private enum Flag
        {
            User,
            Images,
            Channel,
            Bots,
            Invites,
            Match,
            None
        }

        private enum ErrorState
        {
            InvalidParse,
            EndOfLine,
            Unknown,
            NotErrored
        }

        private State _state = State.None;
        private State _lastState = State.None;
        private Flag _flag = Flag.None;
        private ErrorState _error = ErrorState.NotErrored;
        private int index = 0;


        public Task<Optional<SCleanType>> ConvertAsync(string value, CommandContext ctx)
        {
            string[] tokens = value.Split(' ');
            while (index != tokens.Length) { }

            return null;
        }
    }

    public class SCleanType
    {
        public bool Bots { get; init; }
        public bool Images { get; init; }
        public bool Invites { get; init; }
        public string Matches { get; init; }
        public ulong[] Users { get; init; }
        public ulong[] Channels { get; init; }
    }
}