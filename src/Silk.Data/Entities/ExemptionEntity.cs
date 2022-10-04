using Remora.Rest.Core;

namespace Silk.Data.Entities;

public enum ExemptionTarget
{
    Role,
    User,
    Channel
}
public enum ExemptionCoverage
{
	NonExemptMarker = 0,
    EditLogging = 2,
    DeleteLogging = 4,
    AntiPhishing = 8,
    AntiSpam = 16,
    AntiInvite = 32,
    WordBlacklist = 64,
}
public sealed class ExemptionEntity
{
	/// <summary>
	///     The Id of this exemption.
	/// </summary>
	public int ID { get; set; }

	/// <summary>
	///     What this exemption covers.
	/// </summary>
    public ExemptionCoverage Exemption { get; set; }

	/// <summary>
	///     What type of exemption this is.
	/// </summary>
    public ExemptionTarget TargetType { get; set; }

	/// <summary>
	///     The target of the exemption.
	/// </summary>
    public Snowflake TargetID { get; set; }

	/// <summary>
	///     The guild this exemption applies to.
	/// </summary>
    public Snowflake GuildID { get; set; }
}