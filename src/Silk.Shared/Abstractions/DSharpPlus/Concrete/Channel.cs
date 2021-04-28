using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private Channel(DiscordChannel channel) => _channel = channel;

        public async Task<IMessage?> GetMessageAsync(ulong id) => (Message?) await _channel.GetMessageAsync(id);

        public static implicit operator Channel(DiscordChannel channel) => GetOrCacheChannel(channel);


        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Screw you, I'll write DM if I want to.")]
        private static Channel GetOrCacheChannel(DiscordChannel channel)
        {
            var isDM = channel is DiscordDmChannel;
            var isCached = _channels.TryGetValue(channel.Id, out var chn);

            chn ??= new(channel);

            if (!isDM) CacheGuildChannel(channel, chn);
            if (!isCached) _channels.Add(channel.Id, chn);

            return chn;

            static void CacheGuildChannel(DiscordChannel channel, Channel chn)
            {
                _ = Concrete.Guild.Guilds.TryGetValue(channel.Guild.Id, out var guild);

                guild ??= channel.Guild!;
                guild.Channels = guild.Channels.Append(chn).ToList().AsReadOnly();
            }
        }
    }
}