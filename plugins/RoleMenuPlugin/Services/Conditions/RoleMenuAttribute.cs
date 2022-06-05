using System;
using Remora.Commands.Conditions;

namespace RoleMenuPlugin.Conditions;

/// <summary>
/// Indicates a parameter must be a valid role-menu.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class RoleMenuAttribute : ConditionAttribute { }