using Microsoft.AspNetCore.Authorization;

namespace Silk.Dashboard;

public static class DashboardPolicies
{
    public const string BotCreatorRoleName  = "BotCreator";
    public const string TeamMemberRoleName  = "TeamMember";
    public const string TeamMemberPolicyName = "TeamMembersOnly";

    public static AuthorizationPolicy TeamMemberPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                               .RequireRole(TeamMemberRoleName)
                                               .Build();
    }
}