using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record InviteDTO
{
    public int       Id            { get; set; }
    public Snowflake GuildId       { get; set; }
    public Snowflake InviteGuildId { get; set; }
    public string    VanityURL     { get; set; }
}