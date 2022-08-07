using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Components.Dialogs;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;
using Silk.Data.DTOs.Guilds.Config;
using Silk.Data.Entities;
using Silk.Data.MediatR.Greetings;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class ManageGuild
{
    [Inject] private IMediator              Mediator      { get; set; }
    [Inject] private ISnackbar              Snackbar      { get; set; }
    [Inject] private IDialogService         DialogService { get; set; }
    [Inject] private DashboardDiscordClient DiscordClient { get; set; }

    [Parameter] public  string  GuildId { get; set; }

    private Snowflake GuildIdParsed => GuildId.ToSnowflake<Snowflake>();

    private IPartialGuild     _guild;
    private GuildConfigEntity _guildConfig; // Todo: use DTO
    private bool              _requestFailed;
    
    private IReadOnlyList<IRole>         _roles;
    private IReadOnlyList<IChannel>      _channels;
    private IReadOnlyList<IPartialGuild> _guilds;

    protected override async Task OnInitializedAsync()
    {
        await GetGuildFromRestAsync();
        if (_requestFailed) return;
        _ = GetGuildConfigAsync();
        await LoadGuildDataAsync();
        StateHasChanged();
    }

    private static string GetGreetingOptionInfo(GuildGreeting greeting)
    {
        var option = greeting.Option.Humanize(LetterCasing.Title);
        return greeting.Option switch
        {
            GreetingOption.GreetOnJoin or GreetingOption.GreetOnScreening => $"{option} --- {greeting.ChannelID}",
            GreetingOption.GreetOnRole                                    => $"{option} --- {greeting.MetadataID}",
            GreetingOption.DoNotGreet                                     => option,
            _                                                             => ""
        };
    }

    private Task LoadGuildDataAsync()
    {
        return Task.WhenAll
        (
             UpdateGuildsAsync(),
             UpdateChannelAsync(),
             UpdateRolesAsync()
        );
    }

    private async Task UpdateGuildsAsync()
    {
        _guilds = await DiscordClient.GetCurrentUserBotManagedGuildsAsync();
    }

    private async Task UpdateChannelAsync()
    {
        _channels = await DiscordClient.GetBotChannelsAsync(GuildIdParsed);
    }

    private async Task UpdateRolesAsync()
    {
        _roles = await DiscordClient.GetBotRolesAsync(GuildIdParsed);
    }

    private async Task GetGuildConfigAsync()
    {
        _guildConfig = await FetchGuildConfigAsync();
        StateHasChanged();
    }

    private async Task GetGuildFromRestAsync()
    {
        _requestFailed = false;
        _guild = await DiscordClient.GetCurrentUserBotManagedGuildAsync(GuildIdParsed);
        if (_guild is null) _requestFailed = true;
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

    private Task CreateGreetingAsync()
    {
        return OpenGreetingDialogAsync(greeting: new() 
        { 
            GuildID = _guildConfig.GuildID,
        });
    }

    private Task EditGreetingAsync(GuildGreeting greeting)
    {
        return OpenGreetingDialogAsync(greeting: greeting);
    }

    private async Task DeleteGreetingAsync(GuildGreeting greeting)
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
            var deleteResult =  await Mediator.Send(new RemoveGuildGreeting.Request(greeting.Id, GuildIdParsed));
            if (deleteResult.IsSuccess)
            {
                _guildConfig.Greetings.Remove(_guildConfig.Greetings.First(x => x.Id == greeting.Id));
                Snackbar.Add("Successfully deleted greeting!", Severity.Success);
            }
        }

        StateHasChanged();
    }

    private async Task OpenGreetingDialogAsync
    (
        int                  greetingId = 0,
        GuildGreeting? greeting   = null
    )
    {
        // Todo: Handle scrolling?
        var options = new DialogOptions
        {
            CloseOnEscapeKey     = false,
            DisableBackdropClick = true,
            Position             = DialogPosition.Center,
            FullWidth            = true,
            NoHeader             = true,
        };

        var parameters = new DialogParameters
        {
            { "GreetingId", greetingId },
            { "Greeting", greeting },
        };

        var dialog = DialogService.Show<GuildGreetingDialog>("", parameters, options);
        await dialog.Result;

        StateHasChanged();
    }
}