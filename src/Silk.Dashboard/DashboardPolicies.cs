using Microsoft.AspNetCore.Authorization;
using Silk.Dashboard.Providers;

namespace Silk.Dashboard;

public static class DashboardPolicies
{
    public const string BotCreatorRoleName  = "BotCreator";
    public const string TeamMemberRoleName  = "TeamMember";
    public const string TeamMemberPolicyName = "TeamMembersOnly";
    public const string ManageGuildPolicyName = "ManageGuild";

    public static AuthorizationPolicy TeamMemberPolicy()
    {
        return new AuthorizationPolicyBuilder()
              .RequireAuthenticatedUser()
              .RequireRole(TeamMemberRoleName)
              .Build();
    }

    public static AuthorizationPolicy ManageGuildPolicy()
    {
        return new AuthorizationPolicyBuilder()
              .RequireAuthenticatedUser()
              .AddRequirements(new ManageGuildRequirement())
              .Build();
    }
}