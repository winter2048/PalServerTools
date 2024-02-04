using Microsoft.AspNetCore.Components.Authorization;
using PalServerTools.Data;
using PalServerTools.Models;
using PalServerTools.Utils;
using System.Security.Claims;

namespace PalServerTools.Auth
{
    public class ImitateAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieUtil _cookieUtil;
        private readonly PalConfigService _configService;

        public ImitateAuthStateProvider(IHttpContextAccessor httpContextAccessor, ICookieUtil cookieUtil, PalConfigService configService)
        {
            _httpContextAccessor = httpContextAccessor;
            _cookieUtil = cookieUtil;
            _configService = configService;
        }

        bool isLogin = false;
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var cookieValue = _httpContextAccessor.HttpContext?.Request?.Cookies?["auth"];

            isLogin = !string.IsNullOrWhiteSpace(cookieValue) && cookieValue == StringUtil.ComputeMd5Hash(_configService.ToolsConfig.ToolsPassword);
            return await GetState(isLogin);
        }

        private Task<AuthenticationState> GetState(bool isLogin)
        {
            if (isLogin)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,"user"),
                    new Claim(ClaimTypes.Role,"admin")
                };
                var anonymous = new ClaimsIdentity(claims, "auth");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
            }
            else
            {
                var anonymous = new ClaimsIdentity();
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
            }
        }

        public async Task<bool> Login(UserInfo request)
        {
            if (request.Password == _configService.ToolsConfig.ToolsPassword)
            {
                isLogin = true;
                await _cookieUtil.SetValueAsync("auth", StringUtil.ComputeMd5Hash(_configService.ToolsConfig.ToolsPassword), TimeSpan.FromDays(1));
            }
           
            NotifyAuthenticationStateChanged(GetState(isLogin));
            return isLogin;
        }

        public async Task SignOut()
        {
            isLogin = false;
            await _cookieUtil.RemoveAsync("auth");
            NotifyAuthenticationStateChanged(GetState(isLogin));
        }
    }
}
