using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Channel : IChannel
    {
        public ulong Id => _channel.Id;

        private readonly DiscordChannel _channel;

        private Channel(DiscordChannel channel) => _channel = channel;

        public async Task<IMessage?> GetMessageAsync(ulong id) => throw new NotImplementedException("Soon™");

        public static implicit operator Channel(DiscordChannel c) => new(c);

    }
}