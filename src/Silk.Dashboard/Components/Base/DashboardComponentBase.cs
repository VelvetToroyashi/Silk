using BlazorComponentUtilities;
using Microsoft.AspNetCore.Components;

namespace Silk.Dashboard.Components;

public abstract class DashboardComponentBase : ComponentBase
{
    /// <summary>
    /// Classes added after component's classes
    /// </summary>
    [Parameter]
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Styles added after component's styles
    /// </summary>
    [Parameter]
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Attributes added to component that don't match any of its parameters
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> ComponentAttributes { get; set; } = new();

    protected bool IsBusy { get; set; }

    protected virtual CssBuilder ComponentClasses
        => new CssBuilder()
          .AddClass(Class);

    protected virtual StyleBuilder ComponentStyles
        => new StyleBuilder()
          .AddStyle(Style);

    protected async Task ComponentRunAsync
    (
        Func<Task>                      func,
        bool                            callStateHasChanged = true,
        ILogger<DashboardComponentBase> logger              = null
    )
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            await func.Invoke();
        }
        catch (Exception e)
        {
            logger?.LogError(e, "{ErrorMessage}", e.Message);
        }
        finally
        {
            IsBusy = false;
            if (callStateHasChanged) 
                StateHasChanged();
        }
    }
}

public static class BlazorComponentUtilitiesExtensions
{
    public static string Build(this StyleBuilder styleBuilder, bool removeTrailingSemicolon = false) 
        => removeTrailingSemicolon ? styleBuilder.Build().TrimEnd(';') : styleBuilder.Build();
}