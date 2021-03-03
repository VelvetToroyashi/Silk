using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Models;

namespace Silk.Data.MediatR.Handlers
{
    public class TagRequestHandler
    {
        public class CreateHandler : IRequestHandler<TagRequest.Create, Tag>
        {
            private readonly SilkDbContext _db;
            public CreateHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<Tag> Handle(TagRequest.Create request, CancellationToken cancellationToken)
            {
                Tag tag = new()
                {
                    OwnerId = request.OwnerId,
                    GuildId = request.GuildId,
                    Name = request.Name,
                    Content = request.Content,
                    CreatedAt = DateTime.Now
                };

                _db.Tags.Add(tag);
                await _db.SaveChangesAsync(cancellationToken);
                
                return tag;
            }
        }

        public class GetHandler : IRequestHandler<TagRequest.Get, Tag?>
        {
            private readonly SilkDbContext _db;
            public GetHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<Tag?> Handle(TagRequest.Get request, CancellationToken cancellationToken)
            {
                Tag? tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower() 
                                                                   && t.GuildId == request.GuildId, cancellationToken);
                return tag;
            }
        }
    }
}