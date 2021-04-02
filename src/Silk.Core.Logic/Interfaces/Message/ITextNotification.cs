#nullable enable
namespace Silk.Core.Logic.Interfaces.Message
{
    public interface ITextNotification
    {
        public ulong? Id { get; }
        public string? Content { get; }
    }
}