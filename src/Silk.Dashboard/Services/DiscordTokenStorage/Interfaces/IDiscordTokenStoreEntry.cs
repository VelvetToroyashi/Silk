namespace Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

public interface IDiscordTokenStoreEntry
{
    public string?         AccessToken  { get; }
    public string?         RefreshToken { get; }
    public DateTimeOffset? ExpiresAt    { get; }
    public string?         TokenType    { get; }
}