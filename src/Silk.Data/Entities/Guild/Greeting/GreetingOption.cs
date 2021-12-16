namespace Silk.Data.Entities;

/// <summary>
///     Represents a value of when to greet a member.
/// </summary>
public enum GreetingOption
{
    /// <summary>
    ///     Members should not be greeted.
    /// </summary>
    DoNotGreet,

    /// <summary>
    ///     Members should be greeted when given a role.
    /// </summary>
    GreetOnRole,

    /// <summary>
    ///     Members should be greeted when they join the server.
    /// </summary>
    GreetOnJoin,

    /// <summary>
    ///     Members should be greeted when they pass guild member verification.
    /// </summary>
    GreetOnScreening,

    /// <summary>
    ///     Members should be greeted when given access to a channel.
    /// </summary>
    GreetOnChannelAccess,
}