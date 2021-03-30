using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IChannel
    {
        public ulong Id { get; init; }
        public IGuild Guild { get; init; }
        public IReadOnlyList<IMessage> Messages { get; init; }

        public Task<IMessage> SendAsync(IMessage message);
        public Task<IMessage?> GetMessageAsync(ulong id);


        //public static implicit operator IChannel(DiscordChannel channel) => (IChannel)new Channel(channel.Id, channel.Guild, channel.Messages);
    }
}