﻿using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DiscordTokenStore _tokenStore;

    public AuthController(DiscordTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    [HttpPost("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = EnsureLocalRedirectUrl(returnUrl) }, 
                         DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogOut(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _tokenStore.RemoveToken(HttpContext.User.GetUserId());
        return LocalRedirect(EnsureLocalRedirectUrl(returnUrl));
    }

    private string EnsureLocalRedirectUrl(string returnUrl) 
        => Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}