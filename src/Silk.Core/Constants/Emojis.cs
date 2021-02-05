using DSharpPlus.Entities;
using Silk.Extensions;

namespace Silk.Core.Constants
{
    public static class Emojis
    {
        /// <summary>
        /// A wonky workaround to statically initialize emojis.
        /// These are NOT to be used with interactivity. Use the constants instead.
        /// </summary>
        public static readonly DiscordEmoji
            EConfirm = Confirm.ToEmoji(),
            EDecline = Decline.ToEmoji();
        //Todo: load these from JSON
        public const string
            Confirm = "<:check:777724297627172884>",
            Decline = "<:cross:777724316115796011>";

    }
}