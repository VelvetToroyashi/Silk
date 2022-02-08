using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class Profile : ComponentBase
{
    [Inject] public  ISnackbar                Snackbar          { get; set; }
    [Inject] private NavigationManager        NavigationManager { get; set; }
    [Inject] private DiscordRestClientService RestClientService { get; set; }

    private bool _showJoinedGuilds;

    private IUser                        _user;
    private IReadOnlyList<IPartialGuild> _joinedGuilds;
    private IReadOnlyList<IPartialGuild> _ownedGuilds;

    protected override async Task OnInitializedAsync()
    {
        _user         = (await RestClientService.RestClient.GetCurrentUserAsync()).Entity;
        _joinedGuilds = await RestClientService.GetAllGuildsAsync();
        _ownedGuilds  = RestClientService.FilterGuildsByPermission(_joinedGuilds, DiscordPermission.ManageGuild);
    }

    private string CurrentUserAvatar => _user.Avatar.Value;
    // private string CurrentUserAvatar => RestClientService.RestClient.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 256);
    private string CurrentUserName => _user.Username;
    // private string CurrentUserName => RestClientService.RestClient.CurrentUser.Username;
    private string HeaderViewGreeting         => $"Hello, {CurrentUserName}";
    private string JoinedGuildsVisibilityText => $"{(_showJoinedGuilds ? "Hide" : "Show")} Joined Servers"; 

    private void ToggleJoinedGuildsVisibility() => _showJoinedGuilds = !_showJoinedGuilds;

    private void HandleGuildNavigation(IPartialGuild guild)
    {
        var navUrl = $"/Dashboard/ManageGuild/{guild.ID}";
        var canNavigate = guild.Permissions.IsDefined(out var permissionSet) && permissionSet.HasPermission(DiscordPermission.ManageGuild);

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