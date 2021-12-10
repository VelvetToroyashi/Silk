using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

[Table("guild_configs")]
public class GuildConfigEntity
{
    // Database requisites. //
    /// <summary>
    ///     The Primary Key (PK) of the model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    public GuildEntity Guild { get; set; }

    /// <summary>
    ///     The trigger to greet new members, if any. Default to DoNotGreet.
    /// </summary>
    [Obsolete]
    public GreetingOption GreetingOption { get; set; } = GreetingOption.DoNotGreet;

    /// <summary>
    ///     The Id of the role to wait for to be granted while
    /// </summary>
    [Obsolete]
    public ulong VerificationRole { get; set; }

    /// <summary>
    ///     Id of the channel to greet members in.
    /// </summary>
    [Obsolete]
    public ulong GreetingChannel { get; set; }

    /// <summary>
    ///     The text that will be used to greet new members.
    /// </summary>
    [Obsolete]
    public string GreetingText { get; set; }

    /// <summary>
    /// Greetings configured for this guild.
    /// </summary>
    [Column("greetings")]
    public List<GuildGreetingEntity> Greetings { get; set; }
    
    /// <summary>
    ///     A list of disabled commands on this server
    /// </summary>
    public List<DisabledCommandEntity> DisabledCommands { get; set; } = new();
}