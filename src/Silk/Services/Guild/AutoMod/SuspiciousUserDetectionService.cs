using System.Threading.Tasks;
using FuzzySharp;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Interfaces;
using Unidecode.NET;

namespace Silk.Services.Guild;

public class SuspiciousUserDetectionService
{
    private readonly ILogger<SuspiciousUserDetectionService> _logger;
    private readonly IDiscordRestUserAPI                    _users;
    private readonly IInfractionService                     _infractions;
   
    
    private readonly string[] SuspiciousUsernames = new[]
    {
        "ModMail",
        "Bot Moderator",
        "Discord Moderator",
        "Hypesquad Events",
    };
    
    public SuspiciousUserDetectionService(ILogger<SuspiciousUserDetectionService> logger, IDiscordRestUserAPI users, IInfractionService infractions)
    {
        _logger      = logger;
        _infractions = infractions;
        _users  = users;
    }
    
    public async Task<Result> HandleSuspiciousUserAsync(Snowflake guildID, IUser user)
    {
        // TODO: add to config and make toggelable. This will go under phishing settings.
        var detection = IsSuspcectedPhishingUsername(user.Username);
        
        if (!detection.isSuspicious)
            return Result.FromSuccess();

        if (user.IsBot.IsDefined(out var bot) && bot)
        {
            _logger.LogTrace("Suspiciously named bot: {BotName}, similar to {SimilarName}", user.Username, detection.mostSimilarTo);
            return Result.FromSuccess();
        }

        var self = await _users.GetCurrentUserAsync();

        if (!self.IsSuccess)
            return Result.FromError(self.Error);

        // We delete the last day of messages to clear any potential join message.
        var infraction = await _infractions.BanAsync(guildID, user.ID, self.Entity.ID, 1, $"Potential phishing userbot | Username most similar to {detection.mostSimilarTo}");
        
        if (!infraction.IsSuccess)
            return Result.FromError(infraction.Error);
        
        return Result.FromSuccess();
    }
    
    private (bool isSuspicious, string mostSimilarTo) IsSuspcectedPhishingUsername(string username)
    {
        var normalized = username.Unidecode();

        var fuzzy = Process.ExtractOne(normalized, SuspiciousUsernames);

        if (fuzzy.Score > 75)
            _logger.LogTrace("Potentially suspicious Username: {Normalized}, most similar to {FuzzyMatched}, Score: {Score}", normalized, fuzzy.Value, fuzzy.Score);
        
        // This is somewhat arbitrary, and may be adjusted to be more or less sensitive.
        return (fuzzy.Score > 80, fuzzy.Value);
    }
}