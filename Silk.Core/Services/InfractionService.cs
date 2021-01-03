using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using SilkBot.Extensions;

namespace Silk.Core.Services
{
    public class InfractionService
    {
        private readonly ILogger<InfractionService> _logger;
        private readonly IDatabaseService _dbService;
        private readonly ConfigService _configService;
        private readonly ConcurrentQueue<UserInfractionModel> _infractionQueue = new();
        private readonly Thread _infractionThread;
        // Do I *really* have justification to use a full blown thread for this? Eh, absolutely not, but I don't care. ~Velvet //
        public InfractionService(ILogger<InfractionService> logger, IDatabaseService dbService, ConfigService configService)
        {
            _logger = logger;
            _dbService = dbService;
            _configService = configService;
            _infractionThread = new(() => ProcessInfractions());
            InitThread(_infractionThread);
            _logger.LogInformation("Started Infraction Service Thread!");
        }

        public void AddInfraction(UserInfractionModel infraction) => _infractionQueue.Enqueue(infraction);
        
        /// <summary>
        /// Returns a bool indicating whether an infraction should be added to the queue, and thus applied to a user.
        /// </summary>
        /// <param name="member">The user to check.</param>

        public async Task<bool> ShouldAddInfractionAsync(DiscordMember member) => 
            (await _dbService.GetGuildUserAsync(member.Guild.Id, member.Id))?.Flags.Has(UserFlag.AutoModIgnore) ?? false; 

        
        
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
                    Thread.Sleep(100);
                }
            }
        }


    }
}