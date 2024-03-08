using AntDesign;
using Microsoft.AspNetCore.Components;

namespace PalServerTools.Components
{
    public partial class SySliderFormItem : AntInputComponentBase<double>
    {
        [Parameter]
        public double Max { get; set; }

        [Parameter]
        public double Min { get; set; }

        [Parameter]
        public double DefaultValue { get; set; }

        [Parameter]
        public double Step { get; set; }

        [Parameter]
        public string? Label { get; set; }
    }
}
