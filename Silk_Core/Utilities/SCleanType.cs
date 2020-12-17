#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0414 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace SilkBot.Utilities
{
    public class SCleanTypeParser : IArgumentConverter<SCleanType>
    {
        public async Task<Optional<SCleanType>> ConvertAsync(string value, CommandContext ctx)
        {
            var tokens = value.Split(' ').ToList();
            if (!tokens.Any()) return null;

            var sCleanArguments = BuildArguments(tokens);
            var sCleanResult = CreateResult(sCleanArguments);

            return Optional.FromValue(sCleanResult);
        }

        private SCleanType CreateResult(List<SCleanArgument> arguments)
        {
            SCleanType result = new();

            foreach (var argument in arguments)
            {
                switch (argument.Flag)
                {
                    case SCleanArgumentFlag.User:
                        AddIds(argument, result.Users);
                        break;
                    case SCleanArgumentFlag.Images:
                        result.Images = !result.Images;
                        break;
                    case SCleanArgumentFlag.Channel:
                        AddIds(argument, result.Channels);
                        break;
                    case SCleanArgumentFlag.Bots:
                        result.Bots = !result.Bots;
                        break;
                    case SCleanArgumentFlag.Invites:
                        result.Invites = !result.Invites;
                        break;
                    case SCleanArgumentFlag.Match:
                        result.Match = argument.HasValues ? argument.Values[0] as string : null;
                        break;
                    case SCleanArgumentFlag.Unknown:
                        Console.WriteLine($"Couldn't do anything with unrecognized Flag: {argument.Flag}");
                        break;
                }
            }

            return result;
        }

        private static void AddIds(SCleanArgument argument, HashSet<ulong> idsList)
        {
            if (!argument.HasValues) return;

            var stringValues = argument.Values.OfType<string>();
            foreach (var stringValue in stringValues)
                if (ulong.TryParse(stringValue, out var id))
                    idsList.Add(id);
        }

        private List<SCleanArgument> BuildArguments(List<string> tokens)
        {
            List<SCleanArgument> sCleanArguments = new();

            int currIndex = 0;
            while (currIndex < tokens.Count)
            {
                string currentToken = tokens[currIndex];

                if (IsArgument(currentToken))
                {
                    SCleanArgument sCleanArgument = new() {Name = currentToken, Flag = GetArgumentFlag(currentToken)};

                    if (OutOfBounds(currIndex, tokens.Count, 1)) break;

                    var nextToken = tokens[currIndex + 1];

                    if (!IsArgument(nextToken))
                    {
                        int trackingIndex = currIndex + 1;
                        while (trackingIndex < tokens.Count && !IsArgument(tokens[trackingIndex]))
                        {
                            var argValue = tokens[trackingIndex];
                            if (!string.IsNullOrEmpty(argValue))
                            {
                                sCleanArgument.Values.Add(argValue);
                            }

                            trackingIndex++;
                        }

                        currIndex = trackingIndex;
                        sCleanArguments.Add(sCleanArgument);
                    }
                    else
                    {
                        sCleanArguments.Add(sCleanArgument);
                        currIndex++;
                    }
                }
                else
                {
                    currIndex++;
                }
            }

            return sCleanArguments;
        }

        private SCleanArgumentFlag GetArgumentFlag(string argument)
        {
            return argument.ToLowerInvariant() switch
            {
                "-u" => SCleanArgumentFlag.User,
                "-c" => SCleanArgumentFlag.Channel,
                "-i" => SCleanArgumentFlag.Invites,
                "-m" => SCleanArgumentFlag.Match,
                "-b" => SCleanArgumentFlag.Bots,
                "-p" => SCleanArgumentFlag.Images,
                _ => SCleanArgumentFlag.Unknown
            };
        }

        private bool OutOfBounds(int currIndex, int collectionSize, int increment) =>
            currIndex + increment >= collectionSize;

        private bool IsArgument(string token) => token.StartsWith('-');
    }

    public class SCleanArgument
    {
        public string Name { get; set; }
        public SCleanArgumentFlag Flag { get; set; } = SCleanArgumentFlag.Unknown;
        public List<object> Values { get; set; } = new();
        public bool HasValues => Values.Any();

        public override string ToString()
        {
            return
                $"{nameof(Flag)}: {Flag}, {nameof(Name)}: {Name}, " +
                $"{nameof(HasValues)}: {HasValues}, " +
                $"{nameof(Values)}: {string.Join(", ", Values)}";
        }
    }

    public enum SCleanArgumentFlag
    {
        Unknown,
        User,
        Images,
        Channel,
        Bots,
        Invites,
        Match,
    }

    public class SCleanType
    {
        public bool Bots { get; set; }
        public bool Images { get; set; }
        public bool Invites { get; set; }
        public string Match { get; set; }
        public HashSet<ulong> Users { get; init; } = new();
        public HashSet<ulong> Channels { get; init; } = new();

        public override string ToString()
        {
            return
                $"{nameof(Bots)}: {Bots}, {nameof(Images)}: {Images}, " +
                $"{nameof(Invites)}: {Invites}, {nameof(Match)}: {Match}, " +
                $"{nameof(Users)}: {string.Join(", ", Users)}, " +
                $"{nameof(Channels)}: {string.Join(", ", Channels)}";
        }
    }
}