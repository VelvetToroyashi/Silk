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
        private readonly DatabaseService _dbService;
        private readonly ConfigService _configService;
        private readonly ConcurrentQueue<UserInfractionModel> _infractionQueue = new();

        public InfractionService(ILogger<InfractionService> logger, DatabaseService dbService, ConfigService configService) => 
            (_logger, _dbService, _configService) = (logger, dbService, configService);

        
        
        
    }
}