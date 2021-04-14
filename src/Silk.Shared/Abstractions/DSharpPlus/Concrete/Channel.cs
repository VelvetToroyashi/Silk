using System.Threading.Tasks;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Channel : IChannel
    {
        public ulong Id { get; init; }
        /// <inheritdoc />
        public async Task<IMessage?> GetMessageAsync(ulong id)
        {
            return null;
        }

    }
}