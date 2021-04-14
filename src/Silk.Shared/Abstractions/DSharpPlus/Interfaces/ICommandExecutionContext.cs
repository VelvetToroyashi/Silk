using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface ICommandExecutionContext
    {
        public IUser User { get; }
        public IMessage Message { get; }
        public IChannel Channel { get; }
        public IGuild? Guild { get; }
        public string Prefix { get; }

        public Task<IMessage> RespondAsync(string message);
    }
}