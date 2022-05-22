using System.Collections.Generic;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Users;

public record UserDTO(Snowflake ID, IReadOnlyList<Snowflake> Guilds, IReadOnlyList<UserHistoryDTO> History, IReadOnlyList<InfractionDTO> Infractions);