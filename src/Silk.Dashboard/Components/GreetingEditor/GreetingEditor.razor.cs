using Microsoft.AspNetCore.Components;
using MudBlazor;
using Silk.Data.Entities;

namespace Silk.Dashboard.Components;

public partial class GreetingEditor
{
    private MudForm                                 _form;

    private readonly GreetingOption[] _greetingOptions = Enum.GetValues<GreetingOption>();

    [Parameter]
    public GuildGreetingEntity Greeting { get; set; } = new();

    private async Task SubmitAsync()
    {
    }
}