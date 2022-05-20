using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Silk.Extensions.Remora;

namespace Silk.Commands.SlashCommands.Config;

[PublicAPI]
[SlashCommand]
[Group("config")]
[DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages, DiscordPermission.KickMembers)]
public partial class SlashConfig : CommandGroup
{
    
}