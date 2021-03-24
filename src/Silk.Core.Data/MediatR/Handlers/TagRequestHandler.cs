using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Handlers
{
    public class TagRequestHandler
    {
        public class CreateHandler : IRequestHandler<TagRequest.Create, Tag>
        {
            private readonly GuildContext _db;
            public CreateHandler(GuildContext db)
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
                    OriginalTagId = request.OriginalTag?.Id,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow,
                    Aliases = request.OriginalTag is null ? new() : null
                };

                _db.Tags.Add(tag);
                await _db.SaveChangesAsync(cancellationToken);
                
                return tag;
            }
        }

        public class GetHandler : IRequestHandler<TagRequest.Get, Tag?>
        {
            private readonly GuildContext _db;
            public GetHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<Tag?> Handle(TagRequest.Get request, CancellationToken cancellationToken)
            {
                Tag? tag = await _db.Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(t => 
                        t.Name.ToLower() == request.Name.ToLower() 
                        && t.GuildId == request.GuildId, cancellationToken);
                return tag;
            }
        }
        
        public class UpdateHandler : IRequestHandler<TagRequest.Update, Tag>
        {
            private readonly GuildContext _db;
            public UpdateHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<Tag> Handle(TagRequest.Update request, CancellationToken cancellationToken)
            {
                Tag tag = await _db.Tags.FirstAsync(t => t.Name.ToLower() == request.Name.ToLower() &&
                                                         t.GuildId == request.GuildId, cancellationToken);
                tag.Name = request.NewName ?? tag.Name;
                tag.Uses = request.Uses ?? tag.Uses;
                tag.Content = request.Content ?? tag.Content;
                tag.Aliases = request.Aliases ?? tag.Aliases;

                await _db.SaveChangesAsync(cancellationToken);
                return tag;
            }
        }

        public class DeleteHandler : IRequestHandler<TagRequest.Delete>
        {
            private readonly GuildContext _db;
            public DeleteHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<Unit> Handle(TagRequest.Delete request, CancellationToken cancellationToken)
            {
                Tag tag = await _db
                    .Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(t =>
                        t.Name.ToLower() == request.Name.ToLower() &&
                        t.GuildId == request.GuildId, cancellationToken);
                
                Tag[] aliasedTags = await _db
                    .Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .AsSplitQuery()
                    .Where(t => t.OriginalTagId == tag.Id || t.OriginalTag!.OriginalTagId == tag.Id)
                    .ToArrayAsync(cancellationToken);
                
                
                foreach (Tag t in aliasedTags)
                    _db.Tags.Remove(t);
                
                _db.Tags.Remove(tag);
                await _db.SaveChangesAsync(cancellationToken);
                return new();
            }
        }
        
        public class GetByUserHandler : IRequestHandler<TagRequest.GetByUser, IEnumerable<Tag>?>
        {
            private readonly GuildContext _db;
            public GetByUserHandler(GuildContext db)
            {
                _db = db;
            }
            
            public async Task<IEnumerable<Tag>?> Handle(TagRequest.GetByUser request, CancellationToken cancellationToken)
            { 
                Tag[] tags = await _db
                    .Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .Where(t => t.GuildId == request.GuildId && t.OwnerId == request.OwnerId)
                    .ToArrayAsync(cancellationToken);
                
                return tags.Any() ? tags : null; // Return null over empty list //
            }
        }

        public class GetByNameHandler : IRequestHandler<TagRequest.GetByName, IEnumerable<Tag>?>
        {
            private readonly GuildContext _db;
            public GetByNameHandler(GuildContext db)
            {
                _db = db;
            }

            public async Task<IEnumerable<Tag>?> Handle(TagRequest.GetByName request, CancellationToken cancellationToken)
            {
                Tag[] tags = await
                    _db
                        .Tags
                        .Include(t => t.Aliases)
                        .Include(t => t.OriginalTag)
                        .AsSplitQuery()
                        .Where(t => EF.Functions.Like(t.Name.ToLower(), request.Name.ToLower() + '%'))
                        .ToArrayAsync(cancellationToken);

                return tags.Any() ? tags : null;
            }
        }

        public class GetByGuildHandler : IRequestHandler<TagRequest.GetByGuild, IEnumerable<Tag>?>
        {
            private readonly GuildContext _db;
            public GetByGuildHandler(GuildContext db)
            {
                _db = db;
            }

            public async Task<IEnumerable<Tag>?> Handle(TagRequest.GetByGuild request, CancellationToken cancellationToken)
            {
                Tag[] tags = await
                    _db
                    .Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .Where(t => t.GuildId == request.GuildId)
                    .ToArrayAsync(cancellationToken);

                return tags.Any() ? tags : null;
            }
        }
    }
}