using Remora.Rest.Core;

namespace Silk.Remora.SlashCommands;

/// <summary>
/// Represents an identifier for an application command, where the Guild ID may not be present.
/// </summary>
/// <param name="GuildID">The ID of the guild the command is registered for, if any.</param>
/// <param name="CommandID">The ID of the command that was registered.</param>
public record SlashCommandIdentifier(Optional<Snowflake> GuildID, Snowflake CommandID);