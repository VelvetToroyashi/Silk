using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR
{
    public class UpdateGuildRequest : IRequest<Guild>
    {
        public Guild Guild { get; init; }
    }
}