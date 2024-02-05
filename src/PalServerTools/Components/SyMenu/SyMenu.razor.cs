using AntDesign;
using Microsoft.AspNetCore.Components;
using PalServerTools.Auth;
using PalServerTools.Data;
using System;

namespace PalServerTools.Components
{
    public partial class SyMenu : AntDomComponentBase
    {
        [Inject]
        private NavigationManager _nvm { get; set; }
        [Inject]
        private ImitateAuthStateProvider _authStateProvider { get; set; }
        [Inject]
        private ClientConfigService _clientConfigService { get; set; }

        public string CurrentPath { get; set; } = "";

        public SyMenu()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.CurrentPath = new Uri(nvm.Uri).LocalPath;
        }

        public async Task SignOut()
        {
            await _authStateProvider.SignOut();
            _nvm.NavigateTo("/Login");
        }

        public async Task ChangeGlobalTheme()
        {
            await this.ConfigProvider.ChangeGlobalTheme((_clientConfigService.ClientConfig.Theme == "dark" ? GlobalTheme.Light : GlobalTheme.Dark));
            _clientConfigService.ClientConfig.Theme = (_clientConfigService.ClientConfig.Theme == "dark" ? "light" : "dark");
            await _clientConfigService.Save();
        }
    }
}
