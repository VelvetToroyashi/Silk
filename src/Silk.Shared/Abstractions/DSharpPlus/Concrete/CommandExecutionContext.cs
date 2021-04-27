using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    /// <inheritdoc cref="ICommandExecutionContext"/> />
    public class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly IMessageSender _messageSender;

        public CommandExecutionContext(CommandContext context, IMessageSender messageSender) :
            this(context.Message, context.Channel, context.Guild, context.Prefix, messageSender) { }
        public CommandExecutionContext(DiscordMessage message, DiscordChannel channel, DiscordGuild? guild, string prefix, IMessageSender messageSender)
        {
            User = (User) message.Author;
            Message = (Message) message!;
            Channel = (Channel) channel;
            Guild = (Guild) guild!;
            Prefix = prefix;
            _messageSender = messageSender;
        }

        public IUser User { get; }

        public IMessage Message { get; }

        public IChannel Channel { get; }

        public IGuild? Guild { get; }

        public string Prefix { get; }

        public async Task<IMessage> RespondAsync(string message) => await _messageSender.SendAsync(Channel.Id, message);
    }
}