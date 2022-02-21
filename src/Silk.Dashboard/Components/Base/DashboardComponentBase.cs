using BlazorComponentUtilities;
using Microsoft.AspNetCore.Components;

namespace Silk.Dashboard.Components;

public abstract class DashboardComponentBase : ComponentBase
{
    private const int DebugTaskDelayTime = 750;

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

    protected virtual string ComponentClasses
        => new CssBuilder()
          .AddClass(Class)
          .Build();

    protected virtual string ComponentStyles
        => new StyleBuilder()
          .AddStyle(Style)
          .Build();

    protected async Task ComponentRunAsync
    (
        Func<Task>                      func,
        bool                            callStateHasChanged = true,
        ILogger<DashboardComponentBase> logger              = null
    )
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
#if DEBUG
            await Task.Delay(DebugTaskDelayTime);
#endif

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
                await InvokeAsync(StateHasChanged);
        }
    }
}