using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Data.Entities;

/// <summary>
///     Represents an invocation of any of Silk's commands.
/// </summary>
[Table("command_invocations")]
public class CommandInvocationEntity
{
    public long Id { get; set; }

    /// <summary>
    ///     When the command was invoked.
    /// </summary>
    [Required]
    [Column("used_at")]
    public DateTime InvocationTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    ///     What command was invoked, without arguments.
    /// </summary>
    [Required]
    [Column("command_name")]
    public string CommandName { get; init; }
}