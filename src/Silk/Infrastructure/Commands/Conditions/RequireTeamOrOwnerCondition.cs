using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Errors;

namespace Silk.Commands.Conditions;

/// <summary>
/// Determines whether the command is being invoked by someone who is either
/// a member of the team that owns the application, or the owner of the application themselves.
/// </summary>
public class RequireTeamOrOwnerCondition : ICondition<RequireTeamOrOwnerAttribute>
{
    private readonly ICommandContext       _context;
    private readonly IDiscordRestOAuth2API _oauth2;
    
    public RequireTeamOrOwnerCondition(ICommandContext context, IDiscordRestOAuth2API oauth2)
    {
        _context = context;
        _oauth2  = oauth2;
    }
    
    /// <inheritdoc />
    public async ValueTask<Result> CheckAsync(RequireTeamOrOwnerAttribute attribute, CancellationToken ct = default)
    {
        var appResult = await _oauth2.GetCurrentBotApplicationInformationAsync(ct);

        var user = _context switch
        {
            IInteractionCommandContext interaction => interaction.Interaction.User.Value,
            ITextCommandContext command     => command.Message.Author.Value,
        };
        
        if (!appResult.IsSuccess)
            return Result.FromError(appResult.Error);

        var app = appResult.Entity;

        if (app.Team is null)
            return (app.Owner.Value?.ID.IsDefined(out var ID) ?? false) && ID == user.ID
                ? Result.FromSuccess()
                : Result.FromError(new PermissionError("You are not an owner of the application."));
        
        return app.Team.Members.Any(tm => tm.User.ID.IsDefined(out var ID) && ID == user.ID)
            ? Result.FromSuccess()
            : Result.FromError(new PermissionError("You are not a member of the team."));
    }
}