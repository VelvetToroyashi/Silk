using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IChannel
    {
        public ulong Id { get; init; }
        public IGuild Guild { get; init; }
        public IReadOnlyList<IMessage> Messages { get; init; }

        //public static implicit operator IChannel(DiscordChannel channel) => (IChannel)new Channel(channel.Id, channel.Guild, channel.Messages);
    }
}