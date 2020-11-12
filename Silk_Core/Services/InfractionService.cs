using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SilkBot.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using System.Timers;

namespace SilkBot.Services
{
    public class InfractionService
    {
        private readonly ILogger<InfractionService> _logger;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ConcurrentQueue<UserInfractionModel> _infractionQueue = new ConcurrentQueue<UserInfractionModel>();
        private readonly Timer _queueDrainTimer = new Timer(30000);

        public InfractionService(ILogger<InfractionService> logger, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _queueDrainTimer.Elapsed += DrainTimerElapsed;
        }

        private void DrainTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_infractionQueue.IsEmpty)
                _logger.LogInformation("Infraction queue empty! Skipping this round.");
            else
            {
                using var db = _dbFactory.CreateDbContext();
                while (_infractionQueue.TryDequeue(out var infraction))
                    db.Users.First(u => u.Id == infraction.User.Id).Infractions.Add(infraction);
                db.SaveChangesAsync().GetAwaiter();
            }
        }

        public void QueueInfraction(UserInfractionModel infraction) => _infractionQueue.Enqueue(infraction);
        public void AddInfraction(ulong userId)
        {
            var db = _dbFactory.CreateDbContext();
            db.Users.FirstOrDefault(u => u.Id == userId);
        }

        public IEnumerable<UserInfractionModel> GetInfractions(ulong userId)
        {
            var db = _dbFactory.CreateDbContext();
            UserModel user = db.Users.First(u => u.Id == userId);
            return user.Infractions;
        }



    }
}
