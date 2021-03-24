using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Handlers
{
    public class CommandInvokeRequestHandler
    {
        public class AddRequestHandler : IRequestHandler<CommandInvokeRequest.Add>
        {
            private readonly GuildContext _db;
            public AddRequestHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<Unit> Handle(CommandInvokeRequest.Add request, CancellationToken cancellationToken)
            {
                CommandInvocation command = new() { UserId = request.UserId, GuildId = request.GuildId, CommandName = request.CommandName };
                _db.CommandInvocations.Add(command);
                await _db.SaveChangesAsync(cancellationToken);
                
                return new();
            }
        }
        public class GetByUserHandler : IRequestHandler<CommandInvokeRequest.GetByUserId, IEnumerable<CommandInvocation>>
        {

            private readonly GuildContext _db;
            public GetByUserHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<IEnumerable<CommandInvocation>> Handle(CommandInvokeRequest.GetByUserId request, CancellationToken cancellationToken)
            {
                IEnumerable<CommandInvocation> commands = await _db.CommandInvocations.Where(c => c.UserId == request.UserId).ToListAsync(cancellationToken);
                return commands;
            }
        }
        public class GetByGuildHandler : IRequestHandler<CommandInvokeRequest.GetByGuildId, IEnumerable<CommandInvocation>>
        {

            private readonly GuildContext _db;
            public GetByGuildHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<IEnumerable<CommandInvocation>> Handle(CommandInvokeRequest.GetByGuildId request, CancellationToken cancellationToken)
            {
                IEnumerable<CommandInvocation> commands = await _db.CommandInvocations.Where(c => c.UserId == request.GuildId).ToListAsync(cancellationToken);
                return commands;
            }
        }
        
    }
}