using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Services
{
    public class MessageSenderService : IMessageSender
    {
        private readonly DiscordShardedClient _client;
        public MessageSenderService(DiscordShardedClient client)
        {
            _client = client;
        }

        public async Task<IMessage> SendAsync(ulong channelId, string? content)
        {
            DiscordChannel? channel = GetChannel(channelId);
            DiscordMessage message;
            if (channel is not null)
                message = await channel.SendMessageAsync(content);
            else throw new KeyNotFoundException($"Could not find channel with Id {channelId}");

            return (Message) message!;
        }
        public async Task<IMessage> Reply(ulong channelId, ulong messageId, bool mention = false, string? content = null, IEmbed? embed = null)
        {
            throw new NotImplementedException();
        }


        private DiscordChannel? GetChannel(ulong channelId)
        {
            return _client.ShardClients
                .Values
                .SelectMany(g => g.Guilds.Values)
                .SelectMany(g => g.Channels)
                .FirstOrDefault(g => g.Key == channelId)!.Value;
        }
    }
}