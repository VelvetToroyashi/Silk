using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Data.Entities.Guild.Config;

[Table("invite_configs")]
public class InviteConfigEntity
{
    public int Id { get; set; }
    
    /// <summary>
    ///     Blacklist certain invites.
    /// </summary>
    [Column("whitelist_enabled")]
    public bool WhitelistEnabled { get; set; }
    
    /// <summary>
    ///     A list of whitelisted invites.
    /// </summary>
    [Column("whitelist")]
    public List<InviteEntity> Whitelist { get; set; } = new();
    

    /// <summary>
    ///     Represents whether to add an infraction to the user after sending an invite.
    /// </summary>
    [Column("infract")]
    public bool WarnOnMatch { get; set; }

    /// <summary>
    ///     Represents whether to delete a message containing an invite.
    /// </summary>
    [Column("delete")]
    public bool DeleteOnMatch { get; set; }

    /// <summary>
    ///     Whether to scan matched invites. Server must be premium and blacklist invites.
    /// </summary>
    [Column("scan_origin")]
    public bool ScanOrigin { get; set; }
}