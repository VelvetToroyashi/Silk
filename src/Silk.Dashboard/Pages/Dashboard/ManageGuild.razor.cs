using System;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.AspNetCore.Components;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.Models;
using Silk.Dashboard.Services;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages.Dashboard
{
    public partial class ManageGuild : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; }
        [Inject] private IToastService ToastService { get; set; }
        [Inject] private DiscordRestClientService RestClientService { get; set; }

        [Parameter] public string GuildId { get; set; }
        public ulong GuildIdParsed => ulong.Parse(GuildId);

        private readonly GreetingOption[] _greetingOptions = Enum.GetValues<GreetingOption>();
        
        private const uint MaxGreetingTextLength = 2000; // Max Characters for Discord Greeting Text
        private uint RemainingChars => (uint)(MaxGreetingTextLength - _guildConfig.GreetingText.Length);
        private string RemainingCharsClass => RemainingChars < 20 ? "text-danger" : "";

        private bool _requestFailed;

        private DiscordGuild _guild;
        private GuildConfig _guildConfig;

        protected override async Task OnInitializedAsync()
        {
            _guildConfig = await GetGuildConfig();
        }

        private async Task<GuildConfig> GetGuildConfig()
        {
            GuildConfig guildConfig = null;

            try
            {
                _guild = await RestClientService.GetGuildByIdAndPermissions(GuildIdParsed, Permissions.ManageGuild);

                if (_guild is null)
                {
                    /* Todo: Handle this differently? */
                    throw new Exception("The guild requested was either unavailable or the request failed");
                }

                GuildConfig configResponse = await Mediator.Send<GuildConfig>(new GetGuildConfigRequest(GuildIdParsed));
                guildConfig = configResponse ?? (await GetOrCreateNewGuild()).Configuration;

                _requestFailed = false;
            }
            catch (Exception e)
            {
                _requestFailed = true;
            }
            
            return guildConfig;
        }

        private async Task<Guild> GetOrCreateNewGuild()
        {
            return await Mediator.Send(new GetOrCreateGuildRequest(GuildIdParsed,
                StringConstants.DefaultCommandPrefix));
        }

        private async Task SaveChangesAsync()
        {
            var request = new UpdateGuildConfigRequest(GuildIdParsed)
            {
                GreetingOption = _guildConfig.GreetingOption,
                GreetingChannelId = _guildConfig.GreetingChannel,
                VerificationRoleId = _guildConfig.VerificationRole,
                GreetOnScreeningComplete = _guildConfig.GreetingOption is GreetingOption.GreetOnScreening,
                GreetingText = _guildConfig.GreetingText,
                DisabledCommands = _guildConfig.DisabledCommands
            };

            GuildConfig updated = await Mediator.Send(request);
            if (updated is not null)
            {
                ToastService.ShowSuccess("Successfully saved config!");
                _guildConfig = updated;
            }
            else
            {
                ToastService.ShowError("Uh-oh! Something went wrong!");
            }
        }
    }
}