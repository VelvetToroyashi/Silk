using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services.DashboardDiscordClient;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class ManageGuild
{
    [Inject]    private IMediator              Mediator      { get; set; }
    [Inject]    private ISnackbar              Snackbar      { get; set; }
    [Inject]    private DashboardDiscordClient DiscordClient { get; set; }

    [Parameter] public  string  GuildId { get; set; }

    private Snowflake GuildIdParsed => GuildId.ToSnowflake<Snowflake>();
    private bool      RequestFailed { get; set; }

    private const string GenConfigTabId = "gen";
    private const string ModConfigTabId = "mod";

    private bool              _showGreetingEditor;
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
            // TODO: add changes
        };

        return await Mediator.Send(request);
    }

    private async Task SaveChangesAsync()
    {
        await ComponentRunAsync
        (
             async () =>
             {
                 try
                 {
                     var result = await UpdateGuildConfigAsync();
                     var message = result is not null
                         ? "Successfully saved config!"
                         : "Unable to save config changes, please try again";

                     Snackbar.Add(message, Severity.Success);
                 }
                 catch (Exception ex)
                 {
                     Snackbar.Add($"Uh-oh! Something went wrong! - {ex.Message}", Severity.Error);
                 }
             }
        );
    }
}