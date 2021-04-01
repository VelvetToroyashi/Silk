using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Silk.Core.Discord.Utilities
{
    public static class ThrowHelper
    {
        //TODO: Use this more often.
        public static async Task MisconfiguredMuteRole(DiscordChannel channel)
        {
            await channel.SendMessageAsync("Mute role isn't set up!");
            throw new KeyNotFoundException("Mute role not set.");
        }
    }
}