using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Runtime.CompilerServices;

namespace PalServerTools.Components
{
    public partial class SyMenuItem : AntDomComponentBase
    {
        /// <summary>
        /// Set the icon component of button.
        /// </summary>
        [Parameter]
        public string Path { get; set; }

        /// <summary>
        /// Set the icon component of button.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// Set the icon component of button.
        /// </summary>
        [Parameter]
        public string Icon { get; set; }

        [Parameter]
        public bool Active { get; set; }

        /// <summary>
        /// Callback when `Button` is clicked
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }

        private async Task HandleOnClick(MouseEventArgs args)
        {
            if (OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(args);
            }
        }
    }
}
