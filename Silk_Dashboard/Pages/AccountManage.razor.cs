using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Silk_Dashboard.Models.Discord;

namespace Silk_Dashboard.Pages
{
    public partial class AccountManage : ComponentBase
    {
        private string _token;
        private bool _oAuthTokenVisible;
        private const ulong DiscordManageServerPermission = 0x20;

        private DiscordApiUser _user;
        private List<DiscordApiGuild> _guilds;

        private string OAuthTokenVisibility => _oAuthTokenVisible ? "text" : "password";

        protected override async Task OnInitializedAsync()
        {
            _user = await UserService.GetUserInfoAsync(HttpContextAccessor.HttpContext);
            _token = await UserService.GetTokenAsync(HttpContextAccessor.HttpContext);
            _guilds = await UserService.GetUserGuildsAsync(HttpContextAccessor.HttpContext);
        }

        private void ToggleOAuthTokenVisibility()
        {
            _oAuthTokenVisible = !_oAuthTokenVisible;
        }
    }
}