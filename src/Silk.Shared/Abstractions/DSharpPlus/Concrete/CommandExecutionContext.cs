using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    /// <inheritdoc />
    public class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly IMessageSender _messageSender;
        public CommandExecutionContext(DiscordMessage message, DiscordChannel channel, DiscordGuild? guild, IMessageSender messageSender)
        {
            Message = (Message) message!;
            Channel = new Channel() {Id = channel.Id};
            Guild = (Guild) guild!;
            _messageSender = messageSender;
        }

        /// <inheritdoc />
        public IMessage Message { get; internal set; }

        /// <inheritdoc />
        public IChannel Channel { get; internal set; }

        /// <inheritdoc />
        public IGuild? Guild { get; internal set; }

        /// <inheritdoc />
        public async Task<IMessage> RespondAsync(string message)
        {
            return await _messageSender.SendAsync(Channel.Id, message);
        }
        /// <inheritdoc />
        public async Task<IMessage> ReplyAsync(IMessage message)
        {
            return null;
        }
    }
}