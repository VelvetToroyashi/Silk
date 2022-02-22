using Microsoft.AspNetCore.Components;
using MudBlazor;
using Silk.Data.Entities;

namespace Silk.Dashboard.Components;

public partial class GreetingEditor
{
    private const int    MaxMessageLength   = 2000;
    private const string MetaDataIdHelpText = "The ID of the metadata to use for contextual greeting." +
                                              "In the case of greeting when the user receives a role, this will be the ID of the role to check for before greeting." +
                                              "In the case of greeting when a user gains access to a new channel, this will be the ID of the channel to check before greeting.";

    private MudForm _form;

    private readonly GreetingOption[] _greetingOptions = Enum.GetValues<GreetingOption>();

    [Parameter]
    public GuildGreetingEntity Greeting { get; set; } = new();

    private async Task SubmitAsync()
    {
        // await ComponentRunAsync()
    }
}