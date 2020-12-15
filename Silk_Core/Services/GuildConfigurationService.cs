using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SilkBot.Models;

namespace SilkBot.Utilities
{
    public class GuildConfigCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ILogger<GuildConfigCacheService> _logger;

        public GuildConfigCacheService(IMemoryCache cache, IDbContextFactory<SilkDbContext> dbFactory,
            ILogger<GuildConfigCacheService> logger)
        {
            _cache = cache;
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async ValueTask<GuildConfiguration> GetConfigAsync(ulong? guildId)
        {
            if (guildId is null || guildId == 0) return default;
            else if (_cache.TryGetValue(guildId.Value, out GuildConfiguration config)) return config;
            else return await GetConfigFromDatabaseAsync(guildId.Value);
        }

        public async ValueTask<GuildConfiguration> GetConfigFromDatabaseAsync(ulong guildId)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            GuildModel config = await db.Guilds.AsNoTracking().FirstAsync(g => g.Id == guildId);
            if (config is null)
            {
                _logger.LogError("Expected value 'Guild' from databse, received null isntead.");
                return default;
            }

            var guildConfig = new GuildConfiguration(config.WhitelistInvites, config.BlacklistWords, config.AutoDehoist,
                config.GreetMembers, config.MuteRoleId, config.MessageEditChannel, config.GeneralLoggingChannel,
                config.GreetingChannel);
            _cache.CreateEntry(guildId).SetValue(guildConfig)
                  .SetPriority(CacheItemPriority.Low); // Expires in 1 hour if not accessed. //
            return guildConfig;
        }
    }

    public struct GuildConfiguration
    {
        public bool WhitelistsInvites { get; set; }
        public bool WordBlacklistEnabled { get; set; }
        public bool AutoDehoistEnabled { get; set; }
        public bool TrackMemberCountChange { get; set; }
        public ulong? MuteRoleId { get; set; }
        public ulong? MessageEditChannel { get; set; }
        public ulong? LoggingChannel { get; set; }
        public ulong? GreetingChannel { get; set; }

        public List<BlackListedWord> BlacklistedWords { get; }
        public List<WhiteListedLink> WhiteListedLinks { get; }

        public GuildConfiguration
        (
            bool whiteListsInvites = default,
            bool wordBlacklistEnabled = default,
            bool autoDehoistEnabled = default,
            bool trackMemberCountChange = default,
            ulong? muteRoleId = default,
            ulong? messageEditChannel = default,
            ulong? loggingChannel = default,
            ulong? greetingChannel = default
        )
        {
            WhitelistsInvites = whiteListsInvites;
            WordBlacklistEnabled = wordBlacklistEnabled;
            AutoDehoistEnabled = autoDehoistEnabled;
            TrackMemberCountChange = trackMemberCountChange;
            MuteRoleId = muteRoleId;
            MessageEditChannel = messageEditChannel;
            LoggingChannel = loggingChannel;
            GreetingChannel = greetingChannel;
            BlacklistedWords = new List<BlackListedWord>();
            WhiteListedLinks = new List<WhiteListedLink>();
        }
    }
}