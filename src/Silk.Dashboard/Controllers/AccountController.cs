using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    [HttpPost("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var safeReturnUrl = EnsureLocalRedirectUrl(returnUrl);
        var challenge = Challenge(new AuthenticationProperties {RedirectUri = safeReturnUrl},
                                  DiscordAuthenticationDefaults.AuthenticationScheme);
        return challenge;
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut(string returnUrl = "/")
    {
        // This removes the cookie assigned to the user login.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var safeReturnUrl = EnsureLocalRedirectUrl(returnUrl);
        return LocalRedirect(safeReturnUrl);
    }

    /// <summary>
    /// Helper method to protect against Redirect attacks
    /// </summary>
    /// <param name="returnUrl">Optional redirect url</param>
    /// <returns>Safe redirect url. Defaults to "/" if parameter is null or a non-local url</returns>
    private string EnsureLocalRedirectUrl(string returnUrl)
    {
        const string defaultReturnUrl = "/";
        var          temp             = string.IsNullOrWhiteSpace(returnUrl) ? defaultReturnUrl : returnUrl;
        var          newReturnUrl     = Url.IsLocalUrl(temp) ? temp : defaultReturnUrl;
        return newReturnUrl;
    }
}