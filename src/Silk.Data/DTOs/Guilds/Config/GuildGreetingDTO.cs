using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildGreetingDTO
(
    int            ID,
    GreetingOption Option,
    Snowflake      GuildID,
    Snowflake      ChannelID,
    Snowflake?     RoleID,
    string         Message = ""
);