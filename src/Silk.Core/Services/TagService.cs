using System.Collections.Generic;
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
        private readonly Dictionary<GuildKeyNameCombo, Tag> _aliases = new();
        
        private readonly IMediator _mediator;
        public TagService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<object?> GetTagAsync(string tagName, ulong guildId)
        {
            var guild = new GuildKeyNameCombo(tagName, guildId);
            if (_tags.TryGetValue(guild, out var tag))
            {
                return tag;
            }
            else if (_aliases.TryGetValue(guild, out var alias))
            {
                return alias;
            }
            Tag? dbTag = await _mediator.Send(new TagRequest.Get(tagName, guildId));
            Tag? dbAlias = await _mediator.Send(new TagRequest.Get(tagName, guildId));

            if (dbTag is not null)
            {
                return dbTag;
            }
            else
            {
                return dbAlias;
            }

        }
    }        
}