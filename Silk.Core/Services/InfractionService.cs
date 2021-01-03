using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{
    public class InfractionService
    {
        private readonly ILogger<InfractionService> _logger;
        private readonly DatabaseService _dbService;
        private readonly ConfigService _configService;
        private readonly ConcurrentQueue<UserInfractionModel> _infractionQueue = new();
        private readonly Thread _infractionThread;
        // Do I *really* have justification to use a full blown thread for this? Eh, absolutely not, but I don't care. ~Velvet //
        public InfractionService(ILogger<InfractionService> logger, DatabaseService dbService, ConfigService configService)
        {
            _logger = logger;
            _dbService = dbService;
            _configService = configService;
            _infractionThread = new(() => ProcessInfractions());
            InitThread(_infractionThread);
            _logger.LogInformation("Started Infraction Service Thread!");
            
        }

        private void InitThread(Thread thread)
        {
            thread.Name = "Infraction Thread";
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }
        
        private void ProcessInfractions()
        {
            while (true)
            {
                if (!_infractionQueue.IsEmpty)
                {
                    Thread.Sleep(200);
                }
            }
        }


    }
}