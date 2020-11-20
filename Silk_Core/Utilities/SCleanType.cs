using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class SCleanTypeParser : IArgumentConverter<SCleanType>
    {

        
        public Task<Optional<SCleanType>> ConvertAsync(string value, CommandContext ctx)
        {
            try
            {
                string[] split = value.ToLower().Split(' ');
                int index = 0;

                while (index != split.Length)
                {
                   // TODO: Fix this as well //
                }
                return null;
            }
            catch
            {

                return Task.FromResult(Optional.FromNoValue<SCleanType>());
            }        
        }
        private static (SCleanType t, int returnIndex) GetUsers(string[] array)
        {
            var returnValue = new SCleanType(true);
            ulong[] users = array.AsEnumerable().Skip(2).TakeWhile(t => !t.StartsWith('-')).Select(t => ulong.Parse(t)).ToArray();
            returnValue = returnValue with { Users = users };
            return (returnValue, array.IndexOf(users.Last().ToString()));
        }
    }

    public record SCleanType(bool DoUsers = false, ulong[] Users = default, bool ChannelSpecified = false, ulong Channel = default, bool DoBots = false, bool Invites = false, bool MatchString = false, string Match = null);
}
