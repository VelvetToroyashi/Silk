using Remora.Commands.Attributes;
using Remora.Commands.Groups;

namespace Silk.Commands.SlashCommands.Config;

public partial class SlashConfig
{
    [Group("edit")]
    public partial class Edit : CommandGroup { }
}
