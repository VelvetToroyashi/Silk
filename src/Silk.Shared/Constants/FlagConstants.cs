using System.Text.RegularExpressions;
using DSharpPlus;

namespace Silk.Shared.Constants
{
    public static class FlagConstants
    {
        public const Permissions CacheFlag = Permissions.KickMembers | Permissions.ManageMessages;
        public const RegexOptions RegexFlags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;
        public const DiscordIntents Intents = DiscordIntents.Guilds | // Caching
                                              DiscordIntents.GuildMembers | // Auto-mod/Auto-greet
                                              DiscordIntents.DirectMessages | // DM Commands
                                              DiscordIntents.GuildPresences | // Auto-Mod Anti-Status-Invite
                                              DiscordIntents.GuildMessages | // Commands & Auto-Mod
                                              DiscordIntents.GuildMessageReactions | // Role-menu
                                              DiscordIntents.DirectMessageReactions | // Interactivity in DMs
                                              DiscordIntents.GuildVoiceStates;
    }
}