using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
/// A Tag that can be used to display pre-defined text.
/// </summary>
[Table("guild_tags")]
public class TagEntity
{
    /// <summary>
    /// The unique identifier for the Tag.
    /// </summary>
    public int Id   { get; set; }
    
    /// <summary>
    /// How many times the tag has been used.
    /// </summary>
    [Column("uses")]
    public int Uses { get; set; }

    /// <summary>
    /// The name of the tag.
    /// </summary>
    [Column("name")]
    public string Name    { get; set; }
    
    /// <summary>
    /// The content of the tag.
    /// </summary>
    [Column("content")]
    public string Content { get; set; }

    /// <summary>
    /// When the tag was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The ID of the user who created the tag.
    /// </summary>
    [Column("owner_id")]
    public Snowflake OwnerID { get; set; }
    
    /// <summary>
    /// The ID of the guild the tag was created in.
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    /// The ID of the tag's parent, if any.
    /// </summary>
    [Column("parent_id")]
    public int?       OriginalTagId { get; set; }
    
    /// <summary>
    /// The tag's parent, if any.
    /// </summary>
    [Column("parent")]
    public TagEntity? OriginalTag   { get; set; }

    /// <summary>
    /// The tags that are children of this tag.
    /// </summary>
    [Column("aliases")]
    public List<TagEntity>? Aliases { get; set; }
}