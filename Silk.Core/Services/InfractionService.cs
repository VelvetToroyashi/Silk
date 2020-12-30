using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Extensions;

namespace Silk.Core.Services
{
    public class InfractionService
    {
        private readonly ILogger<InfractionService> _logger;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly DatabaseService _dbService;
        private readonly ConcurrentQueue<UserInfractionModel> _infractionQueue = new();
        private readonly Timer _queueDrainTimer = new(30000);

        public InfractionService(ILogger<InfractionService> logger, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _queueDrainTimer.Elapsed += (_, _) => _ = DrainTimerElapsed();
            _queueDrainTimer.Start();
        }

        private async Task DrainTimerElapsed()
        {
            if (!_infractionQueue.IsEmpty)
            {
                 SilkDbContext db = _dbFactory.CreateDbContext();

                while (_infractionQueue.TryDequeue(out UserInfractionModel? infraction))
                {
                    GuildModel guild = db.Guilds.First(g => g.Id == infraction.GuildId);
                    UserModel? user = guild.Users.FirstOrDefault(u => u.Id == infraction.UserId);
                    if (user is null)
                    {
                        user = new UserModel {Flags = UserFlag.KickedPrior, Id = infraction.UserId, Guild = guild};
                        user.Infractions.Add(infraction);
                        await db.Users.AddAsync(user);
                        int changed = await db.SaveChangesAsync();
                        if (changed is 0) _logger.LogWarning("Expected to save [1] entity, but saved [0]");
                    }
                    else
                    {
                        user.Flags.Add(UserFlag.KickedPrior);
                        user.Infractions.Add(infraction);
                        int changed = await db.SaveChangesAsync();
                        if (changed is 0) _logger.LogWarning("Expected to save [1] entity, but saved [0]");
                    }
                }

                _logger.LogDebug("Drained infraction queue.");
            }
        }

        public void QueueInfraction(UserInfractionModel infraction)
        {
            _infractionQueue.Enqueue(infraction);
        }


        public IEnumerable<UserInfractionModel> GetInfractions(ulong userId)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            UserModel? user = db.Users.Include(u => u.Infractions).FirstOrDefault(u => u.Id == userId);
            return user?.Infractions!;
        }
    }
}