using System.Collections.Generic;

namespace Silk.Data.Entities.Guild.Config;


public class InviteConfigEntity
{
    public int Id { get; set; }
    
    /// <summary>
    ///     Blacklist certain invites.
    /// </summary>
    public bool WhitelistEnabled { get; set; }
    
    /// <summary>
    ///     Whether to match only discord.gg/ or all possible invite codes.
    /// </summary>
    public bool UseAggressiveRegex { get; set; }
    
    /// <summary>
    ///     A list of whitelisted invites.
    /// </summary>
    public List<InviteEntity> Whitelist { get; set; } = new();
    
    /// <summary>
    ///     Represents whether to add an infraction to the user after sending an invite.
    /// </summary>
    public bool WarnOnMatch { get; set; }

    /// <summary>
    ///     Represents whether to delete a message containing an invite.
    /// </summary>
    public bool DeleteOnMatch { get; set; }
    
    /// <summary>
    ///     Whether to scan matched invites. Server must be premium and blacklist invites.
    /// </summary>
    public bool ScanOrigin { get; set; }
    
    public int GuildModConfigId { get; set; }
    
    public GuildConfigEntity GuildConfig { get; set; }
}