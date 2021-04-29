using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    /// <inheritdoc cref="IChannel"/>
    public class Channel : IChannel
    {
        public ulong Id => _channel.Id;
        public bool IsPrivate => _channel is DiscordDmChannel;

        public IGuild? Guild => (Guild?) _channel.Guild; // This gets cached, don't worry. //

        private readonly DiscordChannel _channel;
        private static readonly Dictionary<ulong, Channel> _channels = new();

        internal Channel(DiscordChannel channel, bool caching)
        {
            _channel = channel;

            if (!caching && !IsPrivate)
                (Guild!.Channels as List<Channel>)!.Add(this);
        }

        public async Task<IMessage?> GetMessageAsync(ulong id) => (Message?) await _channel.GetMessageAsync(id);

        public static implicit operator Channel(DiscordChannel channel)
        {
            _ = _channels.TryGetValue(channel.Id, out var chn);
            return chn ?? new(channel, false);
        }
    }
}