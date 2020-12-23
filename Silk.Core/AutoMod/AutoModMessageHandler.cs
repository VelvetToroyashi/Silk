using System.Text.RegularExpressions;
using Silk.Core.Services;

namespace Silk.Core.AutoMod
{
    public class AutoModMessageHandler
    {
        private static readonly Regex AGGRESSIVE_REGEX = new(@"discord((app\.com|\.com)\/invite|\.gg\/.+)");
        private static readonly Regex LENIENT_REGEX    = new(@"discord.gg\/invite\/.+");
        private readonly DatabaseService _dbService;


    }
}