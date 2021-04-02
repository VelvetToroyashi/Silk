namespace Silk.Core.Logic.Interfaces.Message
{
    public interface IReplyNotification : ITextNotification
    {
        public ulong ReplyId { get; }
        public bool Mention { get; }
    }
}