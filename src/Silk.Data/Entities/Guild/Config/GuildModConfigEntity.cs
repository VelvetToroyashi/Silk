﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("guild_moderation_config")]
public class GuildModConfigEntity
{
    public int         Id      { get; set; }
    
    /// <summary>
    /// The ID of the guild this config belongs to.
    /// </summary>
    [Column("guild_id")]
    public Snowflake   GuildID { get; set; }
    
    /// <summary>
    /// The Guild this config belongs to.
    /// </summary>
    public GuildEntity Guild   { get; set; }

    /// <summary>
    ///     Id of the role to apply when muting members.
    /// </summary>
    [Column("mute_role")]
    public Snowflake MuteRoleID { get; set; }
    
    // /// <summary>
    // /// Whether to use "native mutes" (Timeouts) when muting members.
    // ///
    // /// This is only applicable if the mute lasts for four weeks or less.
    // /// </summary>
    // [Column("use_native_mute")]
    // public bool UseNativeMute { get; set; }

    /// <summary>
    ///     A list of whitelisted invites.
    /// </summary>
    [Column("whitelisted_invites")]
    public List<InviteEntity> AllowedInvites { get; set; } = new();

    /// <summary>
    ///     The maximum amount of users that can be mentioned in a single message.
    /// </summary>
    [Column("max_user_mentions")]
    public int MaxUserMentions { get; set; }

    /// <summary>
    ///     The maximum amount of roles that can be mentioned in a single role.
    /// </summary>
    [Column("max_role_mentions")]
    public int MaxRoleMentions { get; set; }
    
    /// <summary>
    ///     Blacklist certain invites.
    /// </summary>
    [Column("invite_whitelist_enabled")]
    public bool WhitelistInvites { get; set; }
    

    /// <summary>
    ///     Represents whether to add an infraction to the user after sending an invite.
    /// </summary>
    [Column("infract_on_invite")]
    public bool InfractOnMatchedInvite { get; set; }

    /// <summary>
    ///     Represents whether to delete a message containing an invite.
    /// </summary>
    [Column("delete_invite_messages")]
    public bool DeleteMessageOnMatchedInvite { get; set; }

    /// <summary>
    ///     Whether to match only discord.gg/ or all possible invite codes.
    /// </summary>
    [Column("match_aggressively")]
    public bool UseAggressiveRegex { get; set; }

    /// <summary>
    ///     Whether to use increasingly severe infractions when a user is automatically warned.
    /// </summary>
    [Column("progressive_infractions")]
    public bool ProgressiveStriking { get; set; }
    
    /// <summary>
    ///     All active auto-mod exemptions on the guild.
    /// </summary>
    [Column("exemptions")]
    public List<ExemptionEntity> Exemptions { get; set; } = new();

    /// <summary>
    ///     Whether or not to even scan for phishing links on a server.
    /// </summary>
    [Column("detect_phishing")]
    public bool DetectPhishingLinks { get; set; }

    /// <summary>
    ///     Whether or not phishing links should be deleted.
    /// </summary>
    [Column("delete_detected_phishing")]
    public bool DeletePhishingLinks { get; set; }

    /// <summary>
    ///     Whether to scan matched invites. Server must be premium and blacklist invites.
    /// </summary>
   [Column("scan_invite_origin")]
    public bool ScanInviteOrigin { get; set; }

    /// <summary>
    ///     Gets various logging-related settings.
    /// </summary>
    public GuildLoggingConfigEntity LoggingConfig { get; set; } = new();

    /// <summary>
    ///     A list of steps depending on the number of infractions a <see cref="UserEntity" /> has.
    /// </summary>
    public List<InfractionStepEntity> InfractionSteps { get; set; } = new();

    /// <summary>
    ///     A dictionary containing "named" infraction steps, where the key to the dictionary represents a discriminated action,
    ///     which may have different consequences than another. In the event a key is not found in this dictionary, it should be assumed the action would
    ///     result to <see cref="InfractionType.Strike" />.
    /// </summary>
    public Dictionary<string, InfractionStepEntity> NamedInfractionSteps { get; set; } = new();
}