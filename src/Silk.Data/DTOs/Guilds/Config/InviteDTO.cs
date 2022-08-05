using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record InviteDTO
(
    int       ID,
    Snowflake GuildID,
    Snowflake TargetGuildID,
    string?   VanityUrl
);