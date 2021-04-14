using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface ICommandExecutionContext
    {
        public IMessage Message { get; }
        public IChannel Channel { get; }
        public IGuild? Guild { get; }

        public Task<IMessage> RespondAsync(string message);
        public Task<IMessage> ReplyAsync(IMessage message);
    }
}