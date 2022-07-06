using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Components.Dialogs;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services.DashboardDiscordClient;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class ManageGuild
{
    [Inject] private IMediator              Mediator      { get; set; }
    [Inject] private ISnackbar              Snackbar      { get; set; }
    [Inject] private IDialogService         DialogService      { get; set; }
    [Inject] private DashboardDiscordClient DiscordClient { get; set; }

    [Parameter] public  string  GuildId { get; set; }

    private Snowflake GuildIdParsed => GuildId.ToSnowflake<Snowflake>();
    private bool RequestFailed { get; set; }

    private MudTabs           _tabContainer;
    private IPartialGuild     _guild;
    private GuildConfigEntity _guildConfig;

    protected override Task OnInitializedAsync()
    {
        _ = GetGuildFromRestAsync();
        _ = GetGuildConfigAsync();
        return Task.CompletedTask;
    }

    private async Task GetGuildConfigAsync()
    {
        _guildConfig = await FetchGuildConfigAsync();
        StateHasChanged();
    }

    private async Task GetGuildFromRestAsync()
    {
        RequestFailed = false;
        _guild = await DiscordClient.GetCurrentUserGuildAsync(GuildIdParsed, DiscordPermission.ManageGuild);
        if (_guild is null) RequestFailed = true;
        StateHasChanged();
    }

    private async Task<GuildConfigEntity> FetchGuildConfigAsync()
    {
        var request = new GetOrCreateGuildConfig.Request(GuildIdParsed, StringConstants.DefaultCommandPrefix);
        return await Mediator.Send(request);
    }

    private async Task<GuildConfigEntity> UpdateGuildConfigAsync()
    {
        if (_guildConfig is null) return null;

        var request = new UpdateGuildConfig.Request(GuildIdParsed)
        {
            ScanInvites            = _guildConfig.Invites.ScanOrigin,
            MuteRoleID             = _guildConfig.MuteRoleID,
            UseNativeMute          = _guildConfig.UseNativeMute,
            MaxUserMentions        = _guildConfig.MaxUserMentions,
            MaxRoleMentions        = _guildConfig.MaxRoleMentions,
            BlacklistInvites       = _guildConfig.Invites.WhitelistEnabled,
            UseAggressiveRegex     = _guildConfig.Invites.UseAggressiveRegex,
            EscalateInfractions    = _guildConfig.ProgressiveStriking,
            WarnOnMatchedInvite    = _guildConfig.Invites.WarnOnMatch,
            DetectPhishingLinks    = _guildConfig.DetectPhishingLinks,
            DeletePhishingLinks    = _guildConfig.DeletePhishingLinks,
            DeleteOnMatchedInvite  = _guildConfig.Invites.DeleteOnMatch,
            BanSuspiciousUsernames = _guildConfig.BanSuspiciousUsernames,
            Greetings              = _guildConfig.Greetings,
            LoggingConfig          = _guildConfig.Logging,
            AllowedInvites         = _guildConfig.Invites.Whitelist,
            Exemptions             = _guildConfig.Exemptions,
            InfractionSteps        = _guildConfig.InfractionSteps,
            NamedInfractionSteps   = _guildConfig.NamedInfractionSteps,
        };

        return await Mediator.Send(request);
    }

    private async Task SaveChangesAsync
    (
        string successMessage = "Successfully saved config",
        string errorMessage = "Unable to save config changes, please try again"
    )
    {
        await ComponentRunAsync
        (
             async () =>
             {
                 try
                 {
                     var result = await UpdateGuildConfigAsync();
                     var message = result is not null ? successMessage : errorMessage;
                     Snackbar.Add(message, Severity.Success);
                 }
                 catch (Exception ex)
                 {
                     Snackbar.Add($"Uh-oh! Something went wrong! - {ex.Message}", Severity.Error);
                 }
             }
        );
    }

    private async Task OpenGreetingModal
    (
        int                  greetingId = 0,
        GuildGreetingEntity? greeting   = null
    )
    {
        // Todo: Handle scrolling?
        var options = new DialogOptions
        {
            CloseButton          = true,
            CloseOnEscapeKey     = false,
            DisableBackdropClick = true,
            Position             = DialogPosition.Center,
            FullWidth            = true,
        };

        var parameters = new DialogParameters
        {
            { "GreetingId", greetingId },
            { "Greeting", greeting },
        };

        var dialog = DialogService.Show<CreateGuildGreetingDialog>("", parameters, options);
        await dialog.Result;
        StateHasChanged();
    }

    private async Task DeleteGreetingAsync(GuildGreetingEntity greeting)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", "Confirm delete?" },
            { "ButtonText", "Delete" },
            { "Color", Color.Error }
        };

        var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Medium, };

        var dialog = DialogService.Show<ConfirmationDialog>("Delete", parameters, options);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is true)
        {
            var removed = _guildConfig.Greetings.Remove(greeting);
            if (removed) Snackbar.Add("Removed greeting, make sure to hit 'Save Changes' to persist changes");
        }

        StateHasChanged();
    }
}