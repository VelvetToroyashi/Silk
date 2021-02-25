using System.Text.RegularExpressions;
using DSharpPlus;

namespace Silk.Core.Constants
{
    public static class FlagConstants
    {
        public const Permissions CacheFlag = Permissions.KickMembers | Permissions.ManageMessages;
        public const RegexOptions RegexFlags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;
    }
}