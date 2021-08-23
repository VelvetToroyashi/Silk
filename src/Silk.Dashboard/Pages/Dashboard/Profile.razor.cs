using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Pages.Dashboard
{
    /* Todo: Create DashBotDiscordClient (extend DiscordRestClient using BotToken)  */
    /* Todo: Move methods in DiscordRestClientService to DashDiscordRestClient (regular OAuth2) */
    /* Todo: Inject DashDiscordRestClient into Profile page */
    public partial class Profile : ComponentBase
    {
        [Inject] public ISnackbar Snackbar { get; set; }
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

        private string CurrentUserAvatar => RestClientService.RestClient.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 256);
        private string CurrentUserName => RestClientService.RestClient.CurrentUser.Username;
        private string HeaderViewGreeting => $"Hello, {CurrentUserName}";
        private string JoinedGuildsVisibilityText => $"{(_showJoinedGuilds ? "Hide" : "Show")} Joined Servers"; 

        private void ToggleJoinedGuildsVisibility() => _showJoinedGuilds = !_showJoinedGuilds;

        private void HandleGuildNavigation(DiscordGuild guild)
        {
            var navUrl = $"/Dashboard/ManageGuild/{guild.Id}";
            var canNavigate = (guild.Permissions & Permissions.ManageGuild) is not 0;

            if (!canNavigate)
            {
                Snackbar.Add("<h2>Missing Permissions</h2><br/>" + 
                             "<h3>Please ask an admin or moderator to invite the bot to the desired server 🙂</h3>", 
                    Severity.Info);
                return;
            }
            
            NavigationManager.NavigateTo(navUrl);
        }
    }
}