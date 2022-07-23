using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class Profile
{
    [Inject] public  ISnackbar               Snackbar      { get; set; }
    [Inject] private DashboardDiscordClient  DiscordClient { get; set; }

    private bool _showJoinedGuilds;

    private IUser                                _user;
    private Dictionary<Snowflake, IPartialGuild> _botGuilds;
    private IReadOnlyList<IPartialGuild>         _joinedGuilds;
    private IReadOnlyList<IPartialGuild>         _managedGuilds;

    protected override async Task OnInitializedAsync()
    {
        _user          = await DiscordClient.GetCurrentUserAsync();
        _joinedGuilds  = await DiscordClient.GetCurrentUserGuildsAsync();
        _botGuilds     = await DiscordClient.GetBotGuildsAsync();
        _managedGuilds = await DiscordClient.GetCurrentUserBotManagedGuildsAsync(_joinedGuilds);
    }

    private string CurrentUserName            => _user.Username;
    private string CurrentUserAvatar          => GetUserAvatarUrl();
    private string HeaderViewGreeting         => CurrentUserName;
    private string JoinedGuildsVisibilityText => $"{(_showJoinedGuilds ? "Hide" : "Show")} Joined Servers";

    private string GetUserAvatarUrl()
    {
        var result = CDN.GetUserAvatarUrl(_user, imageSize: 256);
        return result.IsDefined(out var uri) ? uri.ToString() : "";
    }

    private void ToggleJoinedGuildsVisibility() 
        => _showJoinedGuilds = !_showJoinedGuilds;

    private void NavigateToManageGuild(IPartialGuild guild)
    {
        var navUrl = $"/manage-guild/{guild.ID.Value.Value}";
        var canNavigate = guild.Permissions.IsDefined(out var permissionSet) && 
                          permissionSet.HasPermission(DiscordPermission.ManageGuild) && 
                          _botGuilds.ContainsKey(guild.ID.Value);

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