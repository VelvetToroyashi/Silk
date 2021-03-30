namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IUser
    {
        public ulong Id { get; init; }
        public IGuild Guild { get; init; }
    }
}