using System;
using Remora.Commands.Conditions;

namespace Silk.Commands.Conditions;

/// <summary>
/// Indicates that an action cannot be executed against the bot nor the current user.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class NonSelfActionableAttribute : ConditionAttribute { }