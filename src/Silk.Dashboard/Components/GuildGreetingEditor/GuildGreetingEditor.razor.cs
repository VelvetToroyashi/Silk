using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Dashboard.Services;
using Silk.Data.Entities;
using Silk.Data.MediatR.Greetings;
using Silk.Data.MediatR.Guilds;

namespace Silk.Dashboard.Components;

public partial class GuildGreetingEditor
{
    private const int EmbedMessageLengthThreshold = 2000;

    private const string MetaDataIdHelpText = "When greeting on role or join, this will either be the " +
                                              "ID of the role to check for, or the ID of the channel to greet in.";

    private static readonly GreetingOption[] GreetingOptions = Enum.GetValues<GreetingOption>();
    
    [Inject]    public DashboardDiscordClient DiscordClient { get; set; }
    [Inject]    public IMediator              Mediator      { get; set; }
    [Inject]    public ISnackbar              Snackbar      { get; set; }
    [Parameter] public int                    GreetingId    { get; set; }
    [Parameter] public GuildGreetingEntity    Greeting      { get; set; }

    private MudForm _form; 
    private IReadOnlyList<IPartialGuild> _managedGuilds;
    private IReadOnlyList<IChannel> _guildChannels;
    private IReadOnlyList<IRole> _guildRoles;

    private Snowflake GreetingMetadataId
    {
        get => Greeting!.MetadataID ?? default;
        set => Greeting!.MetadataID = value.Value == 0 ? null : value;
    }

    private string SaveButtonText => Greeting.Id > 0 ? "Save Changes" : "Create";

    protected override async Task OnInitializedAsync()
    {
        if (Greeting is null)
        {
            var response = await Mediator.Send(new GetGuildGreeting.Request(GreetingId));
            Greeting = response.IsDefined(out var existingGreeting) ? existingGreeting : new();
        }

        _managedGuilds = await DiscordClient.GetCurrentUserBotManagedGuildsAsync();
        await UpdateGreetingGuildAsync(Greeting.GuildID);
    }

    // Todo: Handle non-update issue of sub select/dropdown menus when switching guilds.
    private async Task UpdateGreetingGuildAsync(Snowflake snowflake)
    {
        Greeting.GuildID = snowflake;
        await UpdateGreetingOptionAsync(Greeting.Option);
    }

    private async Task UpdateGreetingOptionAsync(GreetingOption greetingOption)
    {
        Greeting.Option = greetingOption;
        var task = Greeting.Option switch
        {
            GreetingOption.GreetOnRole      => UpdateRolesAsync(),
            GreetingOption.GreetOnJoin      => UpdateChannelAsync(),
            _                               => Task.CompletedTask
        };
        await task;
    }

    private async Task UpdateChannelAsync()
        => _guildChannels = await DiscordClient.GetBotChannelsAsync(Greeting.GuildID);

    private async Task UpdateRolesAsync() 
        => _guildRoles = await DiscordClient.GetBotRolesAsync(Greeting.GuildID);

    private void UpdateGreeting(GuildGreetingEntity existingGreeting)
    {
        existingGreeting.GuildID    = Greeting.GuildID;
        existingGreeting.ChannelID  = Greeting.ChannelID;
        existingGreeting.Message    = Greeting.Message;
        existingGreeting.Option     = Greeting.Option;
        existingGreeting.MetadataID = Greeting.MetadataID;
    }

    private async Task SubmitAsync()
    {
        await _form.Validate();

        if (!_form.IsValid) return;

        await ComponentRunAsync
        (
             async () =>
             {
                 var guildConfig = await Mediator.Send(new GetGuildConfig.Request(Greeting.GuildID));
                 if (guildConfig is null)
                 {
                     Snackbar.Add($"Could not find a config with ID {Greeting.Id}. <br/>" + 
                                  "Please double check that the guild ID is valid or try again.", Severity.Error);
                     return;
                 }

                 // Todo: Figure out what defines selecting/finding a greeting.
                 var foundGreeting = guildConfig.Greetings
                                                .FirstOrDefault(g => g.Id == Greeting.Id);

                 if (foundGreeting is not null)
                 {
                     UpdateGreeting(foundGreeting);
                     await Mediator.Send(new UpdateGuildConfig.Request(foundGreeting.GuildID) { Greetings = guildConfig.Greetings});
                     Snackbar.Add($"Updated greeting with ID {foundGreeting.Id}", Severity.Success);
                 }
                 else
                 {
                     guildConfig.Greetings.Add(Greeting);
                     await Mediator.Send(new UpdateGuildConfig.Request(Greeting.GuildID) {Greetings = guildConfig.Greetings});
                     Snackbar.Add($"Created greeting with ID {Greeting.Id}", Severity.Success);
                 }
             }
        );
    }
}