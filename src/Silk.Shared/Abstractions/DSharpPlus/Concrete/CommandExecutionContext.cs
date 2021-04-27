using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    /// <inheritdoc cref="ICommandExecutionContext"/> />
    public class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly IMessageSender _messageSender;

        public CommandExecutionContext(CommandContext ctx, IMessageSender messageSender)
        {
            User = (User) ctx.Message.Author;
            CurrentUser = (User) ctx.Guild.CurrentMember;
            Message = (Message) ctx.Message!;
            Channel = (Channel) ctx.Channel;
            Guild = (Guild) ctx.Guild!;
            Prefix = ctx.Prefix;
            _messageSender = messageSender;
        }

        public IUser User { get; }

        public IUser CurrentUser { get; }

        public IMessage Message { get; }

        public IChannel Channel { get; }

        public IGuild? Guild { get; }

        public string Prefix { get; }

        public async Task<IMessage> RespondAsync(string message) => await _messageSender.SendAsync(Channel.Id, message);
    }
}