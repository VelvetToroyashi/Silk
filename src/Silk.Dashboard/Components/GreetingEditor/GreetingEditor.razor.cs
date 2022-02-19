using Microsoft.AspNetCore.Components;
using MudBlazor;
using Remora.Rest.Core;
using Silk.Dashboard.Converters;
using Silk.Data.Entities;

namespace Silk.Dashboard.Components;

public partial class GreetingEditor
{
    private const string IdValidatorError = "Not a valid ID";

    private MudForm                                 _form;

    private readonly GreetingOption[] _greetingOptions = Enum.GetValues<GreetingOption>();

    [Parameter]
    public GuildGreetingEntity Greeting { get; set; } = new();

    private async Task SubmitAsync()
    {
    }
}