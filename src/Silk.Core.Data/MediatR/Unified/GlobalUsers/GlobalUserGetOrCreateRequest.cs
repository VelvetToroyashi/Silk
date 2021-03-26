using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Gets a user whom's data is stored globally, or creates it if it does not exist.
    /// </summary>
    public record GetOrCreateGlobalUserRequest(ulong UserId) : IRequest<GlobalUser>;

    /// <summary>
    /// The default handler for <see cref="GetOrCreateGlobalUserRequest"/>.
    /// </summary>
    public class GetOrCreateGlobalUserHandler { }
}