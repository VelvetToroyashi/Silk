using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class ManageGuild : ComponentBase
{
    [Inject] private IMediator                Mediator          { get; set; }
    [Inject] private ISnackbar                Snackbar          { get; set; }
    [Inject] private NavigationManager        NavManager        { get; set; }
    [Inject] private IDashboardDiscordClient DiscordClient { get; set; }

    [Parameter] 
    public string GuildId { get; set; }
    
    private Snowflake GuildIdParsed => Snowflake.TryParse(GuildId, out var snowflake, Constants.DiscordEpoch)
        ? (Snowflake) snowflake 
        : new();
    
    [Parameter]
    [SupplyParameterFromQuery(Name = "tab")]
    public string? ConfigTab { get; set; }

    /* Todo: Make sure that reading/writing doesn't break anything */
    private volatile bool _busy;
    private volatile bool _savingChanges;
    private volatile bool _requestFailed;

    private const string GenConfigTabId = "gen";
    private const string ModConfigTabId = "mod";

    private IPartialGuild        _guild;
    private GuildConfigEntity    _guildConfig;
    private GuildModConfigEntity _guildModConfig;
    
    private MudTabs              _tabContainer;

    private bool CanShowSaveButton => _guildConfig is not null || _guildModConfig is not null;

    /* Max Characters for Discord Greeting Text */
    private const int  MaxGreetingTextLength = 2000;
    private       long RemainingChars => MaxGreetingTextLength - 0; // Todo: Fix Remaining Chars
    // private long RemainingChars => MaxGreetingTextLength - _guildConfig!.GreetingText.Length;
    private string RemainingCharsClass => RemainingChars < 20 ? "mud-error-text" : "";

    private static string LabelFor(string @string) => @string.Humanize(LetterCasing.Title);

    private static bool PanelIdMatches(MudTabPanel panel, string panelId) 
        => string.Equals(((string)panel.ID).ToLower(), panelId.ToLower(), StringComparison.OrdinalIgnoreCase);

    protected override Task OnInitializedAsync()
    {
        _ = FetchDiscordGuildFromRestAsync();
        _ = GetGuildConfigAsync();
        return Task.CompletedTask;
    }

    private async Task SetTabsAsync()
    {
        if (!string.IsNullOrWhiteSpace(ConfigTab))
        {
            var tab = _tabContainer.Panels.FirstOrDefault(panel => PanelIdMatches(panel, ConfigTab));
            if (tab is null) return;

            if (PanelIdMatches(tab, GenConfigTabId))
            {
                _tabContainer.ActivatePanel(GenConfigTabId);
            }
            else if (PanelIdMatches(tab, ModConfigTabId))
            {
                _tabContainer.ActivatePanel(ModConfigTabId);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task GetGuildConfigAsync()
    {
        _guildConfig = await FetchGuildConfigAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task GetGuildModConfigAsync()
    {
        _guildModConfig = await FetchGuildModConfigAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task FetchDiscordGuildFromRestAsync()
    {
        _requestFailed = false;
        _guild = await DiscordClient.GetGuildByIdAndPermissionAsync(GuildIdParsed, DiscordPermission.ManageGuild);
        if (_guild is null) _requestFailed = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task<GuildConfigEntity> FetchGuildConfigAsync()
    {
        var request = new GetOrCreateGuildConfig.Request(GuildIdParsed,
                                                         StringConstants.DefaultCommandPrefix);
        return await Mediator.Send(request);
    }

    private async Task<GuildModConfigEntity> FetchGuildModConfigAsync()
    {
        var request = new GetOrCreateGuildModConfig.Request(GuildIdParsed, 
                                                            StringConstants.DefaultCommandPrefix);
        return await Mediator.Send(request);
    }

    private async Task<GuildConfigEntity> UpdateGuildConfigAsync()
    {
        if (_guildConfig is null) return null;

        /*var request = new UpdateGuildConfig.Request(GuildIdParsed)
        {
            GreetingOption = _guildConfig.GreetingOption,
            GreetingChannelId = _guildConfig.GreetingChannel,
            VerificationRoleId = _guildConfig.VerificationRole,
            GreetingText = _guildConfig.GreetingText,
            DisabledCommands = _guildConfig.DisabledCommands,
        };*/

        GuildConfigEntity response = null; // Todo: Add MediatR Request + Handler
        // var response = await Mediator.Send(request);
        return response;
    }

    private async Task<GuildModConfigEntity> UpdateGuildModConfigAsync()
    {
        if (_guildModConfig is null) return null;

        var request = new UpdateGuildModConfig.Request(GuildIdParsed)
        {
            MuteRoleID = _guildModConfig.MuteRoleID,
            // EscalateInfractions = _guildModConfig.Es,
            LoggingConfig   = _guildModConfig.LoggingConfig,
            MaxUserMentions = _guildModConfig.MaxUserMentions,
            MaxRoleMentions = _guildModConfig.MaxRoleMentions,
            ScanInvites     = _guildModConfig.ScanInviteOrigin,
            // BlacklistWords = _guildModConfig.BlacklistWords,
            BlacklistInvites = _guildModConfig.WhitelistInvites, // Todo: Rename property?
            // LogMembersJoining = _guildModConfig.LogMemberJoins,
            // LogMembersLeaving = _guildModConfig.LogMemberLeaves,
            UseAggressiveRegex = _guildModConfig.UseAggressiveRegex,
            // WarnOnMatchedInvite = _guildModConfig.InfractOnMatchedInvite, // Todo: Check this is wanted?
            DeleteOnMatchedInvite = _guildModConfig.DeleteMessageOnMatchedInvite,
            InfractionSteps       = _guildModConfig.InfractionSteps,
            AllowedInvites        = _guildModConfig.AllowedInvites,
            // AutoModActions = _guildModConfig.NamedInfractionSteps,
        };

        var response = await Mediator.Send(request);
        return response;
    }

    private async Task SaveChangesAsync()
    {
        var updateGuildConfigTask    = UpdateGuildConfigAsync();
        var updateGuildModConfigTask = UpdateGuildModConfigAsync();

        _savingChanges = true;

        try
        {
            var tasks = new Task[] { updateGuildConfigTask, updateGuildModConfigTask };

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _savingChanges = false;
        }

        var updatedGuildConfig    = updateGuildConfigTask.Result;
        var updatedGuildModConfig = updateGuildModConfigTask.Result;

        if (updatedGuildConfig is not null ||
            updatedGuildModConfig is not null)
        {
            _guildConfig    = updatedGuildConfig;
            _guildModConfig = updatedGuildModConfig;

            Snackbar.Add("Successfully saved config!", Severity.Success);
        }
        else
        {
            Snackbar.Add("Uh-oh! Something went wrong!", Severity.Error);
        }
    }
}