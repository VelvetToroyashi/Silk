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

        private readonly IMediator _mediator;
        public TagService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Tag?> GetTagAsync(string tagName, ulong guildId)
        {
            Tag? dbTag = await _mediator.Send(new TagRequest.Get(tagName, guildId));

            return dbTag;
        }

        public async Task<(bool success, string? reason)> AliasTagAsync(string tagName, string aliasName, ulong guildId, ulong ownerId)
        {
            Tag? tag = await _mediator.Send(new TagRequest.Get(tagName, guildId));
            Tag? alias = await _mediator.Send(new TagRequest.Get(aliasName, guildId));
            
            if (tag is null)
                return (false, "Tag not found!");
            if (alias is not null)
                return (false, "Alias or tag already exists!");
            
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
            
            alias = await _mediator.Send(new TagRequest.Create(aliasName, guildId, ownerId, tag.Content, tag));
            
            tag.Aliases ??= new();
            tag.Aliases.Add(alias);

            await _mediator.Send(new TagRequest.Update(tagName, guildId) {Aliases = tag.Aliases});
            
            return (true, null);
        }
        
        public async Task<(bool success, string? reason)> UpdateTagContentAsync(string tagName, string content, ulong guildId, ulong ownerId)
        {
            Tag? tag = await GetTagAsync(tagName, guildId);
            
            if (await GetTagAsync(tagName, guildId) is null)
            {
                return (false, "Tag not found!");
            }

            if (tag!.OwnerId != ownerId)
            {
                return (false, "You do not have permission to edit this tag! Do you own it?");
            }

            await _mediator.Send(new TagRequest.Update(tagName, guildId) {Content = content});
            return (true, null);
        }

        public async Task<(bool success, string? reason)> CreateTagAsync(string tagName, string content, ulong guildId, ulong ownerId)
        {
            if (await GetTagAsync(tagName, guildId) is not null)
                return (false, "Tag already exists!");
            await _mediator.Send(new TagRequest.Create(tagName, guildId, ownerId, content, null));
            return (true, null);
        }

        public async Task RemoveTagAsync(string tagName, ulong guildId) => 
            await _mediator.Send(new TagRequest.Delete(tagName, guildId));
            

        public async Task<IEnumerable<Tag>?> GetUserTagsAsync(ulong ownerId, ulong guildId) => 
            await _mediator.Send(new TagRequest.GetByUser(guildId, ownerId));
    }        
}