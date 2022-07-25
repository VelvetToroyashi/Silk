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

    private static readonly GreetingOption[] GreetingOptions = Enum.GetValues<GreetingOption>();

    [Inject]    public DashboardDiscordClient DiscordClient { get; set; }
    [Inject]    public IMediator              Mediator      { get; set; }
    [Inject]    public ISnackbar              Snackbar      { get; set; }

    [Parameter] public int                    GreetingId    { get; set; }
    [Parameter] public GuildGreetingEntity    Greeting      { get; set; }
    [Parameter] public EventCallback          OnSubmit      { get; set; }
    [Parameter] public EventCallback          OnCancel      { get; set; }

    private MudForm _form;
    private IReadOnlyList<IRole> _roles;
    private IReadOnlyList<IChannel> _channels;
    private IReadOnlyList<IPartialGuild> _guilds;

    protected override async Task OnInitializedAsync()
    {
        if (GreetingId > 0 && Greeting is null)
        {
            var response = await Mediator.Send(new GetGuildGreeting.Request(GreetingId));
            Greeting = response.IsDefined(out var existingGreeting) ? existingGreeting : new();
        }

        await Task.WhenAll
        (
            UpdateGuildsAsync(),
            LoadChannelsAndRolesAsync()
        );

        StateHasChanged();
    }

    private async Task UpdateGreetingGuildAsync(Snowflake snowflake)
    {
        Greeting.GuildID = snowflake;
        await UpdateGreetingOptionAsync(Greeting.Option);
    }

    // Todo: Make action cancelable.
    private async Task UpdateGreetingOptionAsync(GreetingOption greetingOption)
    {
        Greeting.Option = greetingOption;
        await LoadChannelsAndRolesAsync();
        UpdateGreetingMetadata();
        StateHasChanged();
    }

    private Task LoadChannelsAndRolesAsync()
    {
        return Task.WhenAll
        (
            UpdateChannelAsync(),
            UpdateRolesAsync()
        );
    }

    private void UpdateGreetingMetadata()
    {
        if (_channels?.Count > 0) Greeting.ChannelID = _channels[0].ID;
        if (_roles?.Count > 0) Greeting.MetadataID = _roles[0].ID;
    }

    private async Task UpdateGuildsAsync()
    {
        _guilds = await DiscordClient.GetCurrentUserBotManagedGuildsAsync();
    }

    private async Task UpdateChannelAsync()
    {
        _channels = await DiscordClient.GetBotChannelsAsync(Greeting.GuildID);
    }

    private async Task UpdateRolesAsync()
    {
        _roles = await DiscordClient.GetBotRolesAsync(Greeting.GuildID);
    }

    private string GreetingMessageValidation(string message)
    {
        if (Greeting.Option is not GreetingOption.DoNotGreet && string.IsNullOrWhiteSpace(message))
            return "Message cannot be empty";
        return null;
    }

    private void UpdateGreeting(GuildGreetingEntity greeting)
    {
        greeting.GuildID    = Greeting.GuildID;
        greeting.ChannelID  = Greeting.ChannelID;
        greeting.Message    = Greeting.Message;
        greeting.Option     = Greeting.Option;
        greeting.MetadataID = Greeting.MetadataID;
    }

    private void Cancel()
    {
        if (OnCancel.HasDelegate) 
            OnCancel.InvokeAsync();
    }

    private async Task SubmitAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        await ComponentRunAsync
        (
             async () =>
             {
                 var config = await Mediator.Send(new GetGuildConfig.Request(Greeting.GuildID));
                 if (config is null)
                 {
                     Snackbar.Add($"Could not find a config with ID {Greeting.Id}. <br/>" + 
                                  "Please double check that the guild ID is valid or try again.", Severity.Error);
                 }
                 else
                 {
                    var dbGreeting = config.Greetings.FirstOrDefault(g => g.Id == Greeting.Id);

                    if (dbGreeting is not null)
                    {
                        UpdateGreeting(dbGreeting);
                        await Mediator.Send(new UpdateGuildConfig.Request(dbGreeting.GuildID) { Greetings = config.Greetings});
                        Snackbar.Add($"Updated greeting with ID {dbGreeting.Id}", Severity.Success);
                    }
                    else
                    {
                        config.Greetings.Add(Greeting);
                        await Mediator.Send(new UpdateGuildConfig.Request(Greeting.GuildID) {Greetings = config.Greetings});
                        Snackbar.Add($"Created greeting with ID {Greeting.Id}", Severity.Success);
                    }
                 }

                 if (OnSubmit.HasDelegate) 
                    await OnSubmit.InvokeAsync();
             }
        );
    }
}