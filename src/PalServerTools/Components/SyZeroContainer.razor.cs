using AntDesign;
using Microsoft.AspNetCore.Components;
using PalServerTools.Data;

namespace PalServerTools.Components
{
    public partial class SyZeroContainer: AntDomComponentBase
    {
        [Inject]
        private ClientConfigService _clientConfigService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await _clientConfigService.Init();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await this.ConfigProvider.ChangeGlobalTheme((_clientConfigService?.ClientConfig?.Theme == "dark" ? GlobalTheme.Dark : GlobalTheme.Light));
                this.StateHasChanged();
            }
        }
    }
}
