using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.AutoMod
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

        private void InitializeBlacklistCache()
        {
            var a = _db.GuildConfigs.Where(c => c.BlackListedWords.Count > 0).ToList();
            //Do other shit here
        }
        
    }
}