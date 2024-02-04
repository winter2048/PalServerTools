using AntDesign;
using Microsoft.AspNetCore.Components;
using PalServerTools.Auth;
using System;

namespace PalServerTools.Components
{
    public partial class SyMenu : AntDomComponentBase
    {
        [Inject]
        private  NavigationManager _nvm { get; set; }
        [Inject]
        private  ImitateAuthStateProvider _authStateProvider { get; set; }


        public string THEME { get; set; } = "light";

        public string CurrentPath { get; set; } = "";

        public SyMenu()
        {
        }

        public async Task SignOut()
        {
            await _authStateProvider.SignOut();
            _nvm.NavigateTo("/Login");
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.CurrentPath = new Uri(nvm.Uri).LocalPath;
        }
    }
}
