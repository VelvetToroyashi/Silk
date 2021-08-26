using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Silk.Dashboard.Components
{
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
        /// Rivet for attaching data object to the component
        /// </summary>
        /// <remarks>
        /// Inspiration taken from <b>MudBlazor</b><a href="https://github.com/Garderoben/MudBlazor/blob/dev/src/MudBlazor/Base/MudComponentBase.cs"/>
        /// </remarks>
        [Parameter]
        public object Rivet { get; set; }

        /// <summary>
        /// Attributes added to component that don't match any of its parameters
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> CapturedAttributes { get; set; } = new();

        protected bool IsBusy { get; set; }

        protected async Task ComponentRunAsync(
            Func<Task> func,
            bool callStateHasChanged = true,
            ILogger<DashboardComponentBase> logger = null)
        {
            if (IsBusy) 
                return;

            IsBusy = true;

            try
            {
                #if DEBUG
                await Task.Delay(2000);
                #endif

                await func.Invoke();
            }
            catch (Exception e)
            {
                logger?.LogError(e, e.Message);
            }
            finally
            {
                IsBusy = false;
                if (callStateHasChanged) 
                    StateHasChanged();
            }
        }
    }
}