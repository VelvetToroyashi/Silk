using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config.Mod;
using Silk.Core.Data.Models;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard
{
    public partial class ManageGuild : ComponentBase, IDisposable
    {
        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        [Inject] private NavigationManager NavManager { get; set; }
        [Inject] private DiscordRestClientService RestClientService { get; set; }

        [Parameter] public long GuildId { get; set; }
        private ulong GuildIdParsed => (ulong)GuildId;

        /* Todo: Make sure that reading/writing doesn't break anything */
        private volatile bool _busy;
        private volatile bool _savingChanges;
        private volatile bool _requestFailed;

        private MudTabs _tabContainer;
        private string _pageTabQueryParam;
        private const string GenConfigTabId = "gen";
        private const string ModConfigTabId = "mod";

        private DiscordGuild _guild;
        private GuildConfig _guildConfig;
        private GuildModConfig _guildModConfig;

        private readonly GreetingOption[] _greetingOptions = Enum.GetValues<GreetingOption>();

        private bool CanShowSaveButton => _guildConfig is not null || _guildModConfig is not null;

        /* Max Characters for Discord Greeting Text */
        private const uint MaxGreetingTextLength = 2000;
        private long RemainingChars => MaxGreetingTextLength - _guildConfig!.GreetingText.Length;
        private string RemainingCharsClass => RemainingChars < 20 ? "mud-error-text" : "";

        private static string LabelFor(string @string) 
            => @string.Humanize(LetterCasing.Title);
        private static bool PanelIdMatches(MudTabPanel panel, string panelId) 
            => string.Equals((string)panel.ID, panelId, StringComparison.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            GetQueryStringValues();
            _ = FetchDiscordGuildFromRestAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _ = SetTabsAsync();
                NavManager.LocationChanged += HandleLocationChanged;
            }
        }

        private void GetQueryStringValues()
        {
            NavManager.TryGetQueryString("tab", out _pageTabQueryParam);
        }
        
        private void HandleLocationChanged(object sender, LocationChangedEventArgs e)
        {
            GetQueryStringValues();
            StateHasChanged();
        }

        private async Task LoadAllConfigsAsync()
        {
            var tasks = new Task[] { GetGuildConfigAsync(), GetGuildModConfigAsync(), Task.Delay(4500) };

            _busy = true;

            await Task.WhenAll(tasks);
            
            _busy = false;
        }
        
        private async Task SetTabsAsync()
        {
            var task = LoadAllConfigsAsync();

            if (!string.IsNullOrWhiteSpace(_pageTabQueryParam))
            {
                var tab = _tabContainer.Panels.FirstOrDefault(panel => PanelIdMatches(panel, _pageTabQueryParam));
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

            await task;

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
            _guild = await RestClientService.GetGuildByIdAndPermissions(GuildIdParsed, Permissions.ManageGuild);
            if (_guild is null) _requestFailed = true;
            await InvokeAsync(StateHasChanged);
        }

        private async Task<GuildConfig> FetchGuildConfigAsync()
        {
            return await Mediator.Send(
                new GetOrCreateGuildConfigRequest(GuildIdParsed, StringConstants.DefaultCommandPrefix));
        }

        private async Task<GuildModConfig> FetchGuildModConfigAsync()
        {
            return await Mediator.Send(
                new GetOrCreateGuildModConfigRequest(GuildIdParsed, StringConstants.DefaultCommandPrefix));
        }

        private async Task<GuildConfig> UpdateGuildConfigAsync()
        {
            if (_guildConfig is null) return null;

            var request = new UpdateGuildConfigRequest(GuildIdParsed)
            {
                GreetingOption = _guildConfig.GreetingOption,
                GreetingChannelId = _guildConfig.GreetingChannel,
                VerificationRoleId = _guildConfig.VerificationRole,
                GreetOnScreeningComplete = _guildConfig.GreetingOption is GreetingOption.GreetOnScreening,
                GreetingText = _guildConfig.GreetingText,
                DisabledCommands = _guildConfig.DisabledCommands
            };

            var response = await Mediator.Send(request);
            return response;
        }

        private async Task<GuildModConfig> UpdateGuildModConfigAsync()
        {
            if (_guildModConfig is null) return null;

            var request = new UpdateGuildModConfigRequest(GuildIdParsed)
            {
                MuteRoleId = _guildModConfig.MuteRoleId,
                EscalateInfractions = _guildModConfig.AutoEscalateInfractions,
                LogMessageChanges = _guildModConfig.LogMessageChanges,
                MaxUserMentions = _guildModConfig.MaxUserMentions,
                MaxRoleMentions = _guildModConfig.MaxRoleMentions,
                LoggingChannel = _guildModConfig.LoggingChannel,
                ScanInvites = _guildModConfig.ScanInvites,
                BlacklistWords = _guildModConfig.BlacklistWords,
                BlacklistInvites = _guildModConfig.BlacklistInvites,
                LogMembersJoining = _guildModConfig.LogMemberJoins,
                LogMembersLeaving = _guildModConfig.LogMemberLeaves,
                UseAggressiveRegex = _guildModConfig.UseAggressiveRegex,
                WarnOnMatchedInvite = _guildModConfig.WarnOnMatchedInvite,
                DeleteOnMatchedInvite = _guildModConfig.DeleteMessageOnMatchedInvite,
                InfractionSteps = _guildModConfig.InfractionSteps,
                AllowedInvites = _guildModConfig.AllowedInvites,
                AutoModActions = _guildModConfig.NamedInfractionSteps,
            };

            var response = await Mediator.Send(request);
            return response;
        }

        private async Task SaveChangesAsync()
        {
            var updateGuildConfigTask = UpdateGuildConfigAsync();
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

            var updatedGuildConfig = updateGuildConfigTask.Result;
            var updatedGuildModConfig = updateGuildModConfigTask.Result;

            if (updatedGuildConfig is not null ||
                updatedGuildModConfig is not null)
            {
                _guildConfig = updatedGuildConfig;
                _guildModConfig = updatedGuildModConfig;

                Snackbar.Add("Successfully saved config!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Uh-oh! Something went wrong!", Severity.Error);
            }
        }

        public void Dispose()
        {
            NavManager.LocationChanged -= HandleLocationChanged;
        }
    }
}