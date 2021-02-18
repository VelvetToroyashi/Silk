using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers.Guilds
{
    public class GuildUpdateRequestHandler : IRequestHandler<UpdateGuildRequest, Guild?>
    {

        private readonly SilkDbContext _db;
        public GuildUpdateRequestHandler(SilkDbContext db)
        {
            _db = db;
        }
        
        public async Task<Guild?> Handle(UpdateGuildRequest request, CancellationToken cancellationToken)
        {
            Guild? guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == request.Guild.Id, cancellationToken);


            
            await _db.SaveChangesAsync(cancellationToken);
            return guild;
        }
    }
}