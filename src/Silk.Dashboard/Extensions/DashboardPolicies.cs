using Microsoft.AspNetCore.Authorization;

namespace Silk.Dashboard.Extensions;

public static class DashboardPolicies
{
    public const string TeamMemberPolicy    = "TeamMembersOnly";
    public const string TeamMemberClaimName = "TeamMember";

    public static AuthorizationPolicy IsTeamMemberPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                               .RequireClaim(TeamMemberClaimName)
                                               .Build();
    }
}