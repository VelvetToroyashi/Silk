using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessageSender
    {
        public Task<IMessage> Send(ulong channelId, string content);
    }
}