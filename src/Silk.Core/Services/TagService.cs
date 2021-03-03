using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.Services
{
    public class TagService 
    {
        private record GuildKeyNameCombo(string Name, ulong GuildId);
        private readonly Dictionary<GuildKeyNameCombo, Tag> _tags = new();
        
        private readonly IMediator _mediator;
        public TagService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Tag?> GetTagAsync(string tagName, ulong guildId)
        {
            var guild = new GuildKeyNameCombo(tagName.ToLower(), guildId);
            if (_tags.TryGetValue(guild, out var tag))
            {
                return tag;
            }
            else
            {
                Tag? dbTag = await _mediator.Send(new TagRequest.Get(tagName, guildId));
                if (dbTag is not null) 
                    _tags.Add(new(tagName.ToLower(), guildId), dbTag!);
                return dbTag;
            }
        }

        public async Task<(bool success, string? reason)> AliasTagAsync(string tagName, string aliasName, ulong guildId, ulong ownerId)
        {
            Tag? tag = await _mediator.Send(new TagRequest.Get(tagName, guildId));
            
            if (tag is null)
                return (false, "Tag not found!");
            
            if (tag.OriginalTag is null) // Not an alias
            {
                if (tag.Aliases?.Any(a => string.Equals(a.Name, aliasName, StringComparison.OrdinalIgnoreCase)) ?? false)
                    return (false, "Tag already exists!");
            }
            else
            {
                if (tag.OriginalTag.Aliases!.Any(a => string.Equals(a.Name, aliasName, StringComparison.OrdinalIgnoreCase)))
                    return (false, "Tag already exists!");
            }
            

            var alias = await _mediator.Send(new TagRequest.Create(aliasName, guildId, ownerId, tag.Content, tag));

            _tags.TryAdd(new(aliasName, guildId), alias);
            return (true, null);
        }
        public async Task UpdateTagContentAsync(string tagName, ulong guildId)
        {
            
        }
        
    }        
}