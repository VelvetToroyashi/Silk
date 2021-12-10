using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

public class InviteEntity
{
    public int       Id      { get; set; }
    public Snowflake GuildId { get; set; }

    public Snowflake InviteGuildId { get; set; }
    public string VanityURL     { get; set; }
}