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
        private readonly ConcurrentQueue<(DiscordMember, UserInfractionModel)> _infractionQueue = new();
        private readonly Thread _infractionThread;
        // Do I *really* have justification to use a full blown thread for this? Eh, absolutely not, but I don't care. ~Velvet //
        public InfractionService(ILogger<InfractionService> logger, IDatabaseService dbService, ConfigService configService)
        {
            _logger = logger;
            _dbService = dbService;
            _configService = configService;
            _infractionThread = new(async () => await ProcessInfractions());
            InitThread(_infractionThread);
            _logger.LogInformation("Started Infraction Service Thread!");
        }

        public void AddInfraction(DiscordMember member, UserInfractionModel infraction) => _infractionQueue.Enqueue((member, infraction));
        
        // Return whether or not we should provide and infraction to a member. //

        private async Task<bool> ShouldAddInfractionAsync(DiscordMember member) => 
            (await _dbService.GetGuildUserAsync(member.Guild.Id, member.Id))?.Flags.Has(UserFlag.AutoModIgnore) ?? false;
        private void InitThread(Thread thread)
        {
            thread.Name = "Infraction Thread";
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }
        
        private async Task ProcessInfractions()
        {
            while (_infractionThread.ThreadState is not ThreadState.StopRequested)
            {
                if (!_infractionQueue.IsEmpty)
                {
                    Thread.Sleep(100);
                    continue;
                }
                _ = _infractionQueue.TryDequeue(out (DiscordMember m, UserInfractionModel i) r);
                if (await ShouldAddInfractionAsync(r.m))
                {
                    
                }
            }
        }
    }
}