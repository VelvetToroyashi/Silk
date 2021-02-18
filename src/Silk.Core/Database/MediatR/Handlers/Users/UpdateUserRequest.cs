using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers.Users
{
    public class UpdateUserRequest : IRequest<User>
    {
        
        public UserFlag? Flags { get; init; }   
    }
}