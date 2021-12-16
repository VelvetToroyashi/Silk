using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("disbaled_commands")]
public class DisabledCommandEntity
{
    public int Id { get; set; }
    public string CommandName { get; set; }
    public Snowflake GuildID     { get; set; }
    public GuildEntity Guild       { get; set; }
}