using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Silk.Dashboard.Components
{
    public class DashboardPageBase : DashboardComponentBase
    {
        [Inject] protected IJSRuntime JsRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
    }
}