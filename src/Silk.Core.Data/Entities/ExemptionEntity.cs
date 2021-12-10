using System;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

public enum ExemptionTarget
{
    Role,
    User,
    Channel
}

[Flags]
public enum ExemptionCoverage
{
    MessageEdits,
    MessageDeletes,
    Phishing,
    Spam,
    Invites,
    WordBlacklist
}

public sealed class ExemptionEntity
{
	/// <summary>
	///     The Id of this exemption.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///     What this exemption covers.
	/// </summary>
	[Column("exempt_from")]
    public ExemptionCoverage ExemptFrom { get; set; }

	/// <summary>
	///     What type of exemption this is.
	/// </summary>
	[Column("type")]
    public ExemptionTarget TargetType { get; set; }

	/// <summary>
	///     The target of the exemption.
	/// </summary>
	[Column("target_id")]
    public Snowflake TargetID { get; set; }

	/// <summary>
	///     The guild this exemption applies to.
	/// </summary>
	[Column("guild_id")]
    public Snowflake GuildID { get; set; }
}