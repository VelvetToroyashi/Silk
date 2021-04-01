namespace Silk.Core.Discord.Commands.Moderation.Ban
{
    public sealed class BanFailureReason
    {
        public string FailureReason { get; }
        public const string INSUFFICIENT_BOT_PERMISSIONS = "I can't ban members!";
        public const string INSUFFICIENT_CALLER_PERMISSIONS = "You don't have permission to ban members!";
        public const string UNSUITABLE_HIERARCHY_POSITION = "I cannot ban $user due to roles.";
        public const string MODERATOR_BAN_ATTEMPT = "I can't ban $user; they're a staff member of this guild.";

        public BanFailureReason(string reason)
        {
            FailureReason = reason;
        }
    }
}