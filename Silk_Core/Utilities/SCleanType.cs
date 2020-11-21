using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CommandLine;
using Superpower.Model;

namespace SilkBot.Utilities
{
    public class SCleanTypeParser : IArgumentConverter<SCleanType>
    {

        private enum State { None, OnFlag, Busy, Errored }

        private enum Flag { User, Images, Channel, Bots, Invites, Match, None }

        private enum ErrorState { InvalidParse, EndOfLine, Unknown, NotErrored }

        private State _state = State.None;
        private State _lastState = State.None;
        private Flag _flag = Flag.None;
        private ErrorState _error = ErrorState.NotErrored;
        private int index = 0;


        public Task<Optional<SCleanType>> ConvertAsync(string value, CommandContext ctx)
        {
            var tokens = value.Split(' ');
            while (index != tokens.Length)
            { }

            return null;
        }

    }

    public class SCleanType
    {
        public bool    Bots     { get; init; }
        public bool    Images   { get; init; }
        public bool    Invites  { get; init; }
        public string  Matches  { get; init; }
        public ulong[] Users    { get; init; }
        public ulong[] Channels { get; init; }
    }
}
