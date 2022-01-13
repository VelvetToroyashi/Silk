using System.Collections.Generic;
using Remora.Rest.Core;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin.Types;

public record RoleMenuCreationData(Snowflake UserID, Snowflake GuildID, Snowflake MessageID, RoleMenuOptionModel? Current, IReadOnlyList<RoleMenuOptionModel> Options);