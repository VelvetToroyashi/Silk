using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;

namespace Silk.Core.EventHandlers.MessageAdded.AutoMod
{
    public class AutoModBlacklistHandler
    {
        private readonly Dictionary<ulong, HashSet<string>> _blacklistedWords = new();
        private readonly ILogger<AutoModBlacklistHandler> _logger;
        private readonly SilkDbContext _db;

        public AutoModBlacklistHandler(ILogger<AutoModBlacklistHandler> logger, SilkDbContext db)
        {
            _logger = logger;
            _db = db;
            //Init blacklist
        }

        public async Task Initialize()
        {
            var a = _db.GuildConfigs.Where(c => c.BlackListedWords.Count > 0).ToList();
            //Do other shit here
        }

    }
}