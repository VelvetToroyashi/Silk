using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.MediatR.Unified.Tags;
using Silk.Core.Data.Models;
using Silk.Core.Types;

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
            Tag? dbTag = await _mediator.Send(new GetTagRequest(tagName, guildId));

            return dbTag;
        }

        /// <summary>
        /// Creates a tag that points to another tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to alias to.</param>
        /// <param name="aliasName">The name of the alias.</param>
        /// <param name="guildId">The Id of the guild the tag and alias belong to.</param>
        /// <param name="ownerId">The Id of the owner of the alias.</param>
        /// <returns>A <see cref="TagCreationResult"/> with a provided reason, if the operation was unsuccessful.</returns>
        public async Task<TagCreationResult> AliasTagAsync(string tagName, string aliasName, ulong guildId, ulong ownerId)
        {
            Tag? tag = await _mediator.Send(new GetTagRequest(tagName, guildId));

            if (tag is null)
                return new(false, "Tag not found!");

            Tag? alias = await _mediator.Send(new GetTagRequest(aliasName, guildId));

            if (alias is not null)
                return new(false, "Alias already exists!");


            alias = await _mediator.Send(new CreateTagRequest(aliasName, guildId, ownerId, tag.Content, tag));

            tag.Aliases ??= new();
            tag.Aliases.Add(alias);

            await _mediator.Send(new UpdateTagRequest(tagName, guildId) {Aliases = tag.Aliases});

            return new(true, null);
        }

        /// <summary>
        /// Updates the content of a tag, and corresponding aliases.
        /// </summary>
        /// <param name="tagName">The name of the tag to update (case insensitive).</param>
        /// <param name="content">The content of which to update the tag.</param>
        /// <param name="guildId">The Id of the guild the tag belongs to.</param>
        /// <param name="ownerId">The Id of the owner of the tag.</param>
        /// <returns>A <see cref="TagCreationResult"/> with a provided reason, if the operation was unsuccessful.</returns>
        public async Task<TagCreationResult> UpdateTagContentAsync(string tagName, string content, ulong guildId, ulong ownerId)
        {
            Tag? tag = await GetTagAsync(tagName, guildId);

            if (tag is null)
                return new(false, "Tag not found!");

            if (tag.OriginalTag is not null)
                return new(false, "You cannot edit an alias!");

            if (tag.OwnerId != ownerId)
                return new(false, "You do not have permission to edit this tag! Do you own it?");

            await _mediator.Send(new UpdateTagRequest(tagName, guildId) {Content = content});

            if (tag.Aliases?.Any() ?? false)
            {
                foreach (Tag alias in tag.Aliases)
                {
                    await _mediator.Send(new UpdateTagRequest(alias.Name, guildId) {Content = content});
                }
            }

            return new(true, null);
        }

        /// <summary>
        /// Creates a tag with a given name.
        /// </summary>
        /// <param name="tagName">The name of the tag to create (case-insensitive).</param>
        /// <param name="content">The content of the tag to create.</param>
        /// <param name="guildId">The Id of the guild this tag was created on.</param>
        /// <param name="ownerId">The Id of the user that owns this tag or alias.</param>
        /// <returns>A <see cref="TagCreationResult"/> with a provided reason, if the operation was unsuccessful.</returns>
        public async Task<TagCreationResult> CreateTagAsync(string tagName, string content, ulong guildId, ulong ownerId)
        {
            if (await GetTagAsync(tagName, guildId) is not null)
                return new(false, "Tag already exists!");
            await _mediator.Send(new CreateTagRequest(tagName, guildId, ownerId, content, null));
            return new(true, null);
        }

        /// <summary>
        /// Removes a specific tag from the database. If the tag has aliases, they will be removed as well.
        /// </summary>
        /// <param name="tagName">The name of the tag to remove.</param>
        /// <param name="guildId">The Id of the guild the tag belongs to.</param>
        public async Task RemoveTagAsync(string tagName, ulong guildId) =>
            await _mediator.Send(new DeleteTagRequest(tagName, guildId));


        /// <summary>
        /// Gets a collection of tags a given user owns.
        /// </summary>
        /// <param name="ownerId">The Id of the tag owner.</param>
        /// <param name="guildId">The Id of the guild the tag owner is from.</param>
        /// <returns>A collection of tags the tag owner in question owns, or null, if they do not own any tags.</returns>
        public async Task<IEnumerable<Tag>?> GetUserTagsAsync(ulong ownerId, ulong guildId) =>
            await _mediator.Send(new GetTagByUserRequest(guildId, ownerId));

        /// <summary>
        /// Gets a collection of tags in a guild.
        /// </summary>
        /// <param name="guildId">The Id of the guild.</param>
        /// <returns>A collection of tags in the guild, or null if there are none.</returns>
        public async Task<IEnumerable<Tag>?> GetGuildTagsAsync(ulong guildId) =>
            await _mediator.Send(new GetTagByGuildRequest(guildId));
    }
}