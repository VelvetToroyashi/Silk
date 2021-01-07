using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;

namespace Silk.Core.Services
{
    /// <inheritdoc cref="IInfractionService"/>
    public class InfractionService : IInfractionService
    {
        private readonly IDatabaseService _dbService;
        private readonly ConfigService _configService;
        private readonly ConcurrentQueue<(DiscordMember, UserInfractionModel)> _infractionQueue = new();
        private readonly Thread _infractionThread;
        
        
        // Do I *really* have justification to use a full blown thread for this? Eh, absolutely not, but I don't care. ~Velvet //
        public InfractionService(ILogger<InfractionService> logger, IDatabaseService dbService, ConfigService configService)
        {
            _dbService = dbService;
            _configService = configService;
            _infractionThread = new(async () => await ProcessInfractions());
            InitThread(_infractionThread);
            logger.LogInformation("Started Infraction Service Thread!");
        }

        public async Task<bool> ShouldDeleteMessageAsync(DiscordMember member)
        {
            GuildConfigModel config = await _configService.GetConfigAsync(member.Guild.Id);
            bool deleteInvites = config.DeleteMessageOnMatchedInvite;
            bool isExempt = await ShouldAddInfractionAsync(member);
            return deleteInvites && !isExempt;
        }

        public void AddInfraction(DiscordMember member, UserInfractionModel infraction) => _infractionQueue.Enqueue((member, infraction));
        
        
        // Return whether or not we should provide and infraction to a member. //
        private async Task<bool> ShouldAddInfractionAsync(DiscordMember member)
        {
            UserModel? user = await _dbService.GetGuildUserAsync(member.Guild.Id, member.Id);
            if (user is null)
            {
                await _dbService.GetOrAddUserAsync(member.Guild.Id, member.Id);
                return true;
            }
            else return !user.Flags.HasFlag(UserFlag.InfractionExemption);
        }

        /// <summary>
        /// Used to stop the infraction thread properly.
        /// </summary>
        public void StopInfractionThread() => _infractionThread.Join();
        
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
                if (!_infractionQueue.TryDequeue(out (DiscordMember member, UserInfractionModel infraction) result))
                {
                    Thread.Sleep(100);
                }
                else if (await ShouldAddInfractionAsync(result.member))
                {
                    UserModel user = await _dbService.GetOrAddUserAsync(result.member.Guild.Id, result.member.Id);
                    user.Infractions.Add(result.infraction);
                    await _dbService.UpdateGuildUserAsync(user);
                }
            }
        }
        
        
        
    }
}