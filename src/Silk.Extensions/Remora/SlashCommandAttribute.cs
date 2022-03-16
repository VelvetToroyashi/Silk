using System;

namespace Silk.Extensions.Remora;

/// <summary>
/// Represents that this command group is a slash command and should not be registered as a normal command.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SlashCommandAttribute : Attribute { }