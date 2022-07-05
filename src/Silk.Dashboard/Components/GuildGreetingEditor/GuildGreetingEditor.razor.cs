using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Rest.Core;
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

    [Inject]    public IMediator Mediator   { get; set; }
    [Inject]    public ISnackbar Snackbar   { get; set; }
    [Parameter] public int       GreetingId { get; set; }

    private Snowflake GreetingMetadataID
    {
        get => _greeting.MetadataID ?? default;
        set => _greeting.MetadataID = value.Value == 0 ? null : value;
    }

    private MudForm             _form;
    private GuildGreetingEntity _greeting = new();

    protected override async Task OnInitializedAsync()
    {
        var response = await Mediator.Send(new GetGuildGreeting.Request(GreetingId));
        if (response.IsDefined(out var existingGreeting)) _greeting = existingGreeting;
    }

    private void UpdateGreeting(GuildGreetingEntity existingGreeting)
    {
        existingGreeting.GuildID    = _greeting.GuildID;
        existingGreeting.ChannelID  = _greeting.ChannelID;
        existingGreeting.Message    = _greeting.Message;
        existingGreeting.Option     = _greeting.Option;
        existingGreeting.MetadataID = _greeting.MetadataID;
    }

    private async Task SubmitAsync()
    {
        await _form.Validate();

        if (!_form.IsValid) return;

        await ComponentRunAsync
        (
         async () =>
         {
             var guildConfig = await Mediator.Send(new GetGuildConfig.Request(_greeting.GuildID));
             if (guildConfig is null)
             {
                 Snackbar.Add($"Could not find a config with ID {_greeting.Id}. <br/>" + 
                              "Please double check that the guild ID is valid or try again.", Severity.Error);
                 return;
             }

             var foundGreeting = guildConfig.Greetings
                                            .FirstOrDefault(g => g.ChannelID == _greeting.ChannelID);

             // Updating a Greeting
             if (foundGreeting is not null)
             {
                 UpdateGreeting(foundGreeting);
                 await Mediator.Send(new UpdateGuildConfig.Request(foundGreeting.GuildID) { Greetings = guildConfig.Greetings});
                 Snackbar.Add($"Updated greeting with ID {foundGreeting.Id}", Severity.Success);
             }
             // Creating a Greeting
             else
             {
                 guildConfig.Greetings.Add(_greeting);
                 await Mediator.Send(new UpdateGuildConfig.Request(_greeting.GuildID) {Greetings = guildConfig.Greetings});
                 Snackbar.Add($"Created greeting with ID {_greeting.Id}", Severity.Success);
             }
         }
        );
    }
}