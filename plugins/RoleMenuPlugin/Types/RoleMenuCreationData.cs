using System.Collections.Generic;
using Remora.Rest.Core;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin.Types;

public record RoleMenuCreationData(Snowflake UserID, Snowflake MessageID, RoleMenuCreationState CreationState, IReadOnlyList<RoleMenuOptionModel> Options);