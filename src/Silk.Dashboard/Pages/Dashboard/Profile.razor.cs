using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Components;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Pages.Dashboard
{
    public partial class Profile : ComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private DiscordRestClientService RestClientService { get; set; }

        private bool _showJoinedGuilds;
        
        private IReadOnlyList<DiscordGuild> _joinedGuilds;
        private IReadOnlyList<DiscordGuild> _ownedGuilds;

        protected override async Task OnInitializedAsync()
        {
            _joinedGuilds = await RestClientService.GetAllGuildsAsync();
            _ownedGuilds = RestClientService.FilterGuildsByPermission(_joinedGuilds, Permissions.ManageGuild);
        }

        private string CurrentUserAvatar => RestClientService.RestClient.CurrentUser.GetAvatarUrl(ImageFormat.Auto);
        private string CurrentUserName => RestClientService.RestClient.CurrentUser.Username;
        private string HeaderViewGreeting => $"Hello, {CurrentUserName}";

        private void ToggleJoinedGuildsVisibility() => _showJoinedGuilds = !_showJoinedGuilds;

        private void HandleGuildNavigation(DiscordGuild guild, bool canNavigate)
        {
            var navUrl = $"/Dashboard/ManageGuild/{guild.Id}";
            if (!canNavigate)
            {
                ToastService.ShowInfo("Please ask an admin or moderator to" +
                                      " invite the bot to the desired server 🙂", "Missing Permissions");
                return;
            }
            
            NavigationManager.NavigateTo(navUrl);
        }
    }
}