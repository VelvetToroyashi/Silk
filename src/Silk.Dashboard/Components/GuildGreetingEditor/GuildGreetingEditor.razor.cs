using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Dashboard.Services;
using Silk.Data.DTOs.Guilds.Config;
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
    [Parameter] public GuildGreeting          Greeting      { get; set; }
    [Parameter] public EventCallback          OnSubmit      { get; set; }
    [Parameter] public EventCallback          OnCancel      { get; set; }

    private MudForm _form;
    private IReadOnlyList<IRole> _roles;
    private IReadOnlyList<IChannel> _channels;
    private IReadOnlyList<IPartialGuild> _guilds;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(UpdateGuildsAsync(), LoadChannelsAndRolesAsync());
        StateHasChanged();
    }

    private async Task UpdateGreetingGuildAsync(Snowflake snowflake)
    {
        Greeting.GuildID = snowflake;
        await UpdateGreetingOptionAsync(Greeting.Option);
    }

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
        // Todo: Handle display and selection change when either list is empty 
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

    private void UpdateGreeting(GuildGreeting greeting)
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
        await ComponentRunAsync
        (
             async () =>
             {
                 await _form.Validate();
                 if (!_form.IsValid) return;

                 var isUpdatingGreeting = Greeting.Id > 0;
                 var result = isUpdatingGreeting
                     ? await Mediator.Send(new UpdateGuildGreeting.Request(Greeting))
                     : await Mediator.Send(new AddGuildGreeting.Request(Greeting));

                 var messageData = result.IsDefined(out var resultGreeting)
                     ? ($"Greeting #{resultGreeting.Id} {(isUpdatingGreeting ? "updated" : "created")} successfully", Severity.Success)
                     : ($"Failed to complete request - {result.Error}", Severity.Error);

                 Snackbar.Add(messageData.Item1, messageData.Item2);

                 if (OnSubmit.HasDelegate)
                     await OnSubmit.InvokeAsync();
             }
        );
    }
}