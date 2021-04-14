using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    /// <inheritdoc cref="ICommandExecutionContext"/> />
    public class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly IMessageSender _messageSender;
        public CommandExecutionContext(DiscordMessage message, DiscordChannel channel, DiscordGuild? guild, string prefix, IMessageSender messageSender)
        {
            User = (User) message.Author;
            Message = (Message) message!;
            Channel = new Channel() {Id = channel.Id};
            Guild = (Guild) guild!;
            Prefix = prefix;
            _messageSender = messageSender;
        }

        /// <inheritdoc />
        public IUser User { get; }
        /// <inheritdoc />
        public IMessage Message { get; }

        /// <inheritdoc />
        public IChannel Channel { get; }

        /// <inheritdoc />
        public IGuild? Guild { get; }

        public string Prefix { get; }

        /// <inheritdoc />
        public async Task<IMessage> RespondAsync(string message)
        {
            return await _messageSender.SendAsync(Channel.Id, message);
        }
    }
}