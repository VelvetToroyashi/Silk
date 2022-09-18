using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;
public class InviteEntity
{
    public int       ID      { get; set; }
    public Snowflake GuildId { get; set; }
    public Snowflake InviteGuildId { get; set; }
    public string VanityURL     { get; set; }
}