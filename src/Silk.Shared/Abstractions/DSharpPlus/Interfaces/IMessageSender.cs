using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessageSender
    {
        public Task<IMessage> SendAsync(ulong channelId, string content);
    }
}