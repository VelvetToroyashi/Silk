using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;

namespace Silk.Dashboard.Components;

public partial class GreetingEditor
{
    private const int MaxMessageLength = 2000;

    private const string MetaDataIdHelpText = "The ID of the metadata to use for contextual greeting."              +
                                              "In the case of greeting when the user receives a role, this "        +
                                              "will be the ID of the role to check for before greeting."            +
                                              "In the case of greeting when a user gains access to a new channel, " +
                                              "this will be the ID of the channel to check before greeting.";

    private static readonly GreetingOption[] GreetingOptions = Enum.GetValues<GreetingOption>();

    private MudForm _form;
    
    [Inject]
    public IMediator Mediator { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Parameter]
    public GuildGreetingEntity Greeting { get; set; } = new();

    private bool ExistingGreetingFound(GuildConfigEntity config, out GuildGreetingEntity existingGreeting)
    {
        existingGreeting = config.Greetings.FirstOrDefault(g => g.ChannelID == Greeting.ChannelID);
        return existingGreeting != null;
    }

    private async Task SubmitAsync()
    {
        await ComponentRunAsync
            (
             async () =>
             {
                 var config = await Mediator.Send(new GetGuildConfig.Request(Greeting.GuildID));

                 var foundGreeting = ExistingGreetingFound(config, out var existingGreeting);

                 if (foundGreeting)
                 {
                     existingGreeting.GuildID    = Greeting.GuildID;
                     existingGreeting.ChannelID  = Greeting.ChannelID;
                     existingGreeting.Message    = Greeting.Message;
                     existingGreeting.Option     = Greeting.Option;
                     existingGreeting.MetadataID = Greeting.MetadataID;

                     await Mediator.Send(new UpdateGuildConfig.Request(Greeting.GuildID, config.Greetings));
                     Snackbar.Add($"Updated greeting with ID {Greeting.Id}`", Severity.Success);
                 }

                 // Creating a Greeting
                 else
                 {
                     config.Greetings.Add(Greeting);

                     await Mediator.Send(new UpdateGuildConfig.Request(Greeting.GuildID, config.Greetings));
                     Snackbar.Add($"Created greeting with ID {Greeting.Id}`", Severity.Success);
                 }
             }
            );
    }
}