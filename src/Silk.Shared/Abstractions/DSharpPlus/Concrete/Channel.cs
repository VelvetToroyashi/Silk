using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Channel : IChannel
    {
        private readonly DiscordClient _client;
        public Channel(DiscordClient client, DiscordChannel channel)
        {
            _client = client;
            Id = channel.Id;
        }


        public ulong Id { get; init; }
        public IGuild? Guild { get; init; }
        public IReadOnlyList<IMessage> Messages { get; init; }
        public async Task<IMessage> SendAsync(IMessage message)
        {
            DiscordChannel channel = await _client.GetChannelAsync(Id);
            if (message.Reply is null)
            {
                return ((Message) await channel.SendMessageAsync(message.Content))!;
            }
            else
            {
                var builder = new DiscordMessageBuilder();
                builder.WithReply(message.Reply.Id);
                builder.WithContent(message.Content);
                return ((Message) await channel.SendMessageAsync(builder))!;
            }
        }
        public async Task<IMessage> SendAsync(string message)
        {
            throw new System.NotImplementedException();
        }
        public async Task<IMessage?> GetMessageAsync(ulong id)
        {
            throw new System.NotImplementedException();
        }
    }
}