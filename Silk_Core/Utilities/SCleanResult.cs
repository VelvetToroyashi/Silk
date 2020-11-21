using System;
using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class SCleanTypeParser : IArgumentConverter<SCleanResult>
    {

        private enum State { Searching, OnFlag, Busy, Errored }

        private enum Flag { User, Images, Channel, Bots, Invites, Match, None, Unknown }

        private enum ErrorState { InvalidParse, EndOfLine, Unknown, NotErrored }

        private State _state = State.Searching;
        private State _lastState = State.Searching;
        private Flag _flag = Flag.None;
        private ErrorState _error = ErrorState.NotErrored;
        private int index = 0;
        
        private string CurrentToken = "";
        private string PreviousToken = "";
        private string NextToken = "";

        private List<string> _parsingErrors = new();


        // !sclean <count> [-u <users>] [-p] [-c <channel>] [-b] [-i] [-m <match>]
        // !sclean 4 -u 123 123 123 -p -c 123 -b -i -m abc
        /*
         * Tokens:
         * -u
         * 123
         * 123
         * 123
         * -p
         * -c
         * -b
         * -i
         * -m
         * abc
         */
        public async Task<Optional<SCleanResult>> ConvertAsync(string value, CommandContext ctx)
        {
            SCleanResult sCleanResult = new() { Users = new List<ulong>(), Channels = new List<ulong>() };
            
            var tokens = value.Split(' ');

            while (index < tokens.Length)
            {
                var nextToken = "";
                var previousToken = "";
                var currentToken = tokens[index];
                
                if (index - 1 > -1) previousToken = tokens[index - 1];
                if (index + 1 < tokens.Length) nextToken = tokens[index + 1];

                switch (_state)
                {
                    case State.Searching:
                    {
                        if (currentToken[0] == '-')
                        {
                            _state = State.OnFlag;
                            _flag = currentToken[1] switch
                            {
                                'u' => Flag.User,
                                'c' => Flag.Channel,
                                'i' => Flag.Invites,
                                'm' => Flag.Match,
                                'b' => Flag.Bots,
                                'p' => Flag.Images,
                                _ => Flag.Unknown
                            };

                            index++;
                        }
                        break;
                    }
                    case State.OnFlag:
                    {
                        var strValue = currentToken;
                        
                        // Switch on type of flag
                        switch (_flag)
                        {
                            case Flag.User:
                                var userIdParsed = ulong.TryParse(strValue, out ulong userId);
                                if (userIdParsed) sCleanResult.Users.Add(userId);
                                else _parsingErrors.Add($"Could not parse: {strValue} to userId");
                                break;
                            case Flag.Images:
                                sCleanResult.Images = !sCleanResult.Images;
                                break;
                            case Flag.Channel:
                                var channelIdParsed = ulong.TryParse(strValue, out ulong channelId);
                                if (channelIdParsed) sCleanResult.Channels.Add(channelId);
                                else _parsingErrors.Add($"Could not parse: {strValue} to userId");
                                break;
                            case Flag.Bots:
                                sCleanResult.Bots = !sCleanResult.Bots;
                                break;
                            case Flag.Invites:
                                sCleanResult.Invites = !sCleanResult.Invites;
                                break;
                            case Flag.Match:
                                sCleanResult.Matches = strValue;
                                break;
                            case Flag.None:
                                break;
                            case Flag.Unknown:
                                _state = State.Errored;
                                _parsingErrors.Add("Flag not known");
                                break;
                        }

                        if (FoundBreakChar(nextToken, '-'))
                        {
                            _state = State.Searching;
                        }

                        index++;
                        break;
                    }
                    case State.Errored:
                        // Todo: Replace with actual error handling
                        Console.WriteLine($"Failed to parse. The following errors occurred: {string.Join(", ", _parsingErrors)}");
                        return null;
                }
            }

            return Optional.FromValue(sCleanResult);
        }

        private bool FoundBreakChar(string currentToken, char breakChar)
        {
            if (string.IsNullOrEmpty(currentToken)) return false;
            if (currentToken[0] == breakChar) return true;
            return false;
        }
    }

    public class SCleanResult
    {
        public bool    Bots     { get; set; }
        public bool    Images   { get; set; }
        public bool    Invites  { get; set; }
        public string  Matches  { get; set; }
        public List<ulong> Users    { get; init; }
        public List<ulong> Channels { get; init; }
        
        public override string ToString()
        {
            return $"{nameof(Bots)}: {Bots}, {nameof(Images)}: {Images}, {nameof(Invites)}: {Invites}, {nameof(Matches)}: {Matches}, " +
                   $"{nameof(Users)}: {string.Join(", ", Users)}, {nameof(Channels)}: {string.Join(", ", Channels)}";
        }
    }
}
