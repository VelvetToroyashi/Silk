using Microsoft.AspNetCore.Authorization;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Providers;

public class ManageGuildRequirement : IAuthorizationRequirement {}

public class ManageGuildAuthorizationHandler : AuthorizationHandler<ManageGuildRequirement, string>
{
    private readonly DashboardDiscordClient _discordClient;

    public ManageGuildAuthorizationHandler(DashboardDiscordClient discordClient)
    {
        _discordClient = discordClient;
    }

    protected override async Task HandleRequirementAsync
    (
        AuthorizationHandlerContext context,
        ManageGuildRequirement      requirement,
        string                      guildIdStr
    )
    {
        var userIdStr = context.User.GetUserId();
        if (!DiscordSnowflake.TryParse(userIdStr ?? string.Empty, out var userIdSnowflake))
        {
            context.Fail(new (this, "User ID either was not present or could not be parsed."));
            return;
        }

        if (!DiscordSnowflake.TryParse(guildIdStr, out var guildIdSnowflake))
        {
            context.Fail(new (this, "Could not parse guild ID."));
            return;
        }

        if (await _discordClient.CurrentUserCanManageGuildAsync(guildIdSnowflake.Value))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new (this, $"User {userIdSnowflake} does not have the needed permissions to manage guild {guildIdSnowflake}."));
    }
}