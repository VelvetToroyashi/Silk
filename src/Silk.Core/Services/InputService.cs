using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services
{
    public class InputService : IInputService
    {

        public async Task<string?> GetStringInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }
        public async Task<bool?> GetBoolInputFromMessageAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }
        public async Task<DiscordEmoji?> GetReactionInputAsync(ulong userId, ulong channelId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }
    }
}