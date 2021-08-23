using System.Security.Claims;

namespace Silk.Dashboard.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal.Identity?.IsAuthenticated ?? false;
        }
    }
}