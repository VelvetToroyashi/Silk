using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Utilities
{
    public static class ThrowHelper
    {
        //TODO: Use this more often.
        public static async Task MisconfiguredMuteRole(ulong channelId, IMessageSender sender)
        {
            await sender.SendAsync(channelId, "Mute role isn't set up!");
            throw new KeyNotFoundException("Mute role not set.");
        }
    }
}