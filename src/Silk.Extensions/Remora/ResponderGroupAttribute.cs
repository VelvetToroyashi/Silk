using System;
using Remora.Discord.Gateway.Responders;

namespace Silk;

/// <summary>
/// An attribute that represents a responder should be placed in a specific group (Early, Normal, or Late).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ResponderGroupAttribute : Attribute
{
    public ResponderGroupAttribute(ResponderGroup group)
    {
        Group = group;
    }
    
    /// <summary>
    /// The group the responder should be registered in.
    /// </summary>
    public ResponderGroup Group { get; init; }
}