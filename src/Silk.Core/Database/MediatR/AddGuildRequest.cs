using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR
{
    public class AddGuildRequest : IRequest<Guild>
    {
        public ulong GuildId { get; init; }
    }
}