using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface ICommandContext
    {
        public IMessage Message { get; internal set; }
        public IChannel Channel { get; internal set; }
        public IGuild? Guild { get; internal set; }

        public Task<IMessage> RespondAsync(IMessage message);
        public Task<IMessage> ReplyAsync(IMessage message);
    }
}