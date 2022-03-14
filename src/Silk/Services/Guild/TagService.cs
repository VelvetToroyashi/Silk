/*using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Tags;
using Silk.Types;

namespace Silk.Services.Guild;

public sealed class TagService
{
    private readonly IMediator _mediator;
    public TagService(IMediator mediator) => _mediator = mediator;

    public async Task<TagEntity?> GetTagAsync(string tagName, Snowflake guildID) => await _mediator.Send(new GetTagRequest(tagName, guildID));

    /// <summary>
    ///     Creates a tag that points to another tag.
    /// </summary>
    /// <param name="tagName">The name of the tag to alias to.</param>
    /// <param name="aliasName">The name of the alias.</param>
    /// <param name="guildID">The Id of the guild the tag and alias belong to.</param>
    /// <param name="ownerID">The Id of the owner of the alias.</param>
    /// <returns>A <see cref="TagCreationResult" /> with a provided reason, if the operation was unsuccessful.</returns>
    public async Task<TagCreationResult> AliasTagAsync(string tagName, string aliasName, Snowflake guildID, Snowflake ownerID)
    {
        TagEntity? tag = await _mediator.Send(new GetTagRequest(tagName, guildID));

        if (tag is null)
            return new(false, "Tag not found!");

        TagEntity? alias = await _mediator.Send(new GetTagRequest(aliasName, guildID));

        if (alias is not null)
            return new(false, "Alias already exists!");

        await _mediator.Send(new CreateTagRequest(aliasName, guildID, ownerID, tag.Content, tag));
        return new(true, null);
    }

    /// <summary>
    ///     Updates the content of a tag, and corresponding aliases.
    /// </summary>
    /// <param name="tagName">The name of the tag to update (case insensitive).</param>
    /// <param name="content">The content of which to update the tag.</param>
    /// <param name="guildID">The Id of the guild the tag belongs to.</param>
    /// <param name="ownerID">The Id of the owner of the tag.</param>
    /// <returns>A <see cref="TagCreationResult" /> with a provided reason, if the operation was unsuccessful.</returns>
    public async Task<TagCreationResult> UpdateTagContentAsync(string tagName, string content, Snowflake guildID, Snowflake ownerID)
    {
        TagEntity? tag = await GetTagAsync(tagName, guildID);

        if (tag is null)
            return new(false, "Tag not found!");

        if (tag.OriginalTag is not null)
            return new(false, "You cannot edit an alias!");

        if (tag.OwnerID != ownerID)
            return new(false, "You do not have permission to edit this tag! Do you own it?");

        await _mediator.Send(new UpdateTagRequest(tagName, guildID) { Content = content });

        if (!(tag.Aliases?.Any() ?? false))
            return new(true, null);

        foreach (TagEntity alias in tag.Aliases)
            await _mediator.Send(new UpdateTagRequest(alias.Name, guildID) { Content = content });

        return new(true, null);
    }


    /// <summary>
    ///     Claims a tag or alias, changing its owner.
    /// </summary>
    /// <param name="tag">The tag to update.</param>
    /// <param name="guildID">The Id of the guild the tag belongs to.</param>
    /// <param name="newOwnerID">The Id of the tag's new owner.</param>
    public Task ClaimTagAsync(string tag, Snowflake guildID, Snowflake newOwnerID) 
        => _mediator.Send(new UpdateTagRequest(tag, guildID) { OwnerID = newOwnerID });

    /// <summary>
    ///     Creates a tag with a given name.
    /// </summary>
    /// <param name="tagName">The name of the tag to create (case-insensitive).</param>
    /// <param name="content">The content of the tag to create.</param>
    /// <param name="guildID">The Id of the guild this tag was created on.</param>
    /// <param name="ownerID">The Id of the user that owns this tag or alias.</param>
    /// <returns>A <see cref="TagCreationResult" /> with a provided reason, if the operation was unsuccessful.</returns>
    public async Task<TagCreationResult> CreateTagAsync(string tagName, string content, Snowflake guildID, Snowflake ownerID)
    {
        if (await GetTagAsync(tagName, guildID) is not null)
            return new(false, "Tag already exists!");
        await _mediator.Send(new CreateTagRequest(tagName, guildID, ownerID, content, null));
        return new(true, null);
    }

    /// <summary>
    ///     Removes a specific tag. If the tag has aliases, they will be removed as well.
    /// </summary>
    /// <param name="tagName">The name of the tag to remove.</param>
    /// <param name="guildID">The Id of the guild the tag belongs to.</param>
    public Task RemoveTagAsync(string tagName, Snowflake guildID) => _mediator.Send(new DeleteTagRequest(tagName, guildID));

    public Task<IEnumerable<TagEntity>> SearchTagsAsync(string tagName, Snowflake guildID) => _mediator.Send(new GetTagByNameRequest(tagName, guildID));

    /// <summary>
    ///     Gets a collection of tags a given user owns.
    /// </summary>
    /// <param name="ownerID">The Id of the tag owner.</param>
    /// <param name="guildID">The Id of the guild the tag owner is from.</param>
    /// <returns>A collection of tags the tag owner in question owns, or null, if they do not own any tags.</returns>
    public Task<IEnumerable<TagEntity>> GetUserTagsAsync(Snowflake ownerID, Snowflake guildID) => _mediator.Send(new GetTagByUserRequest(guildID, ownerID));

    /// <summary>
    ///     Gets a collection of tags in a guild.
    /// </summary>
    /// <param name="guildID">The Id of the guild.</param>
    /// <returns>A collection of tags in the guild, or null if there are none.</returns>
    public Task<IEnumerable<TagEntity>> GetGuildTagsAsync(Snowflake guildID) => _mediator.Send(new GetTagByGuildRequest(guildID));
}*/