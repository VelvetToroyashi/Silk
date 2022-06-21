using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard;

public partial class ManageGuild
{
    [Inject]    private IMediator               Mediator      { get; set; }
    [Inject]    private ISnackbar               Snackbar      { get; set; }
    [Inject]    private IDashboardDiscordClient DiscordClient { get; set; }
    [Parameter] public  string                  GuildId       { get; set; }

    private Snowflake GuildIdParsed => GuildId.ToSnowflake<Snowflake>();

    private bool _savingChanges;
    private bool _requestFailed;

    private const string GenConfigTabId = "gen";
    private const string ModConfigTabId = "mod";

    private IPartialGuild        _guild;
    private GuildConfigEntity    _guildConfig;
    private GuildModConfigEntity _guildModConfig;

    private MudTabs _tabContainer;

    private bool CanShowSaveButton => _guildConfig is not null || _guildModConfig is not null;

    protected override Task OnInitializedAsync()
    {
        _ = FetchDiscordGuildFromRestAsync();
        _ = GetGuildConfigAsync();
        return Task.CompletedTask;
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
        _guild = await DiscordClient.GetCurrentUserGuildByIdAndPermissionAsync(GuildIdParsed, DiscordPermission.ManageGuild);
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