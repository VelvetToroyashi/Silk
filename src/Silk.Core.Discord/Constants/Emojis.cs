namespace Silk.Core.Discord.Constants
{
    public static class Emojis
    {
        public static ulong ConfirmId { get; } = 777724297627172884;
        public static ulong DeclineId { get; } = 777724316115796011;

        //Todo: load these from JSON
        public static readonly string
            Confirm = $"<:y:{ConfirmId}>",
            Decline = $"<:n:{DeclineId}>";
    }
}