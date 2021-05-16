using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

namespace Silk.Core.Utilities
{
    public static class ThrowHelper
    {
        //TODO: Use this more often.
        public static async Task MisconfiguredMuteRole(ulong channelId, CommandContext sender)
        {
            await sender.RespondAsync("Mute role isn't set up!");
            throw new KeyNotFoundException("Mute role not set.");
        }
    }
}