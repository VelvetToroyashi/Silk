using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Silk_Dashboard.Data
{
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private IDataProtectionProvider Provider { get; }

        public AccountController(IDataProtectionProvider provider)
        {
            Provider = provider;
        }
        
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            var challenge = Challenge(new AuthenticationProperties {RedirectUri = returnUrl}, "Discord");
            return challenge;
        }

        [HttpGet]
        public async Task<IActionResult> LogOut(string returnUrl = "/")
        {
            // This removes the cookie assigned to the user login.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect(returnUrl);
        }
    }
}