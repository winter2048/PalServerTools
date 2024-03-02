using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace PalServerTools.Utils
{
    public class CookieUtil : ICookieUtil
    {
        private readonly IJSRuntime JSRuntime;

        private readonly CookieConfigOptions _settings;

        public CookieUtil(IJSRuntime jsRuntime, IOptions<CookieConfigOptions> options)
        {
            JSRuntime = jsRuntime;
            _settings = options.Value;
        }

        public Task SetValueAsync(string key, object value, TimeSpan? span = null, string? path = null, string? domain = null, bool? secure = null)
        {
            return SetValueAsync(key, JsonConvert.SerializeObject(value), span, path, domain, secure);
        }

        public async Task SetValueAsync(string key, string value, TimeSpan? span = null, string? path = null, string? domain = null, bool? secure = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _settings.Path;
            }

            if (!span.HasValue)
            {
                span = _settings.DefaultExpire;
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = _settings.Domain;
            }

            if (!secure.HasValue)
            {
                secure = _settings.IsSecure;
            }

            string text = ((span.HasValue && span.Value.Ticks > 0) ? DateToUTC(span.Value) : "");
            List<string> list = new List<string>();
            list.Add(key + "=" + value);
            list.Add("expires=" + text);
            list.Add("path=" + path);
            if (!string.IsNullOrWhiteSpace(domain))
            {
                list.Add("domain=" + domain);
            }

            if (secure.HasValue && secure.Value)
            {
                list.Add("secure");
            }

            await SetCookie(string.Join(";", list));
        }

        public async Task RemoveAsync(string key, string? path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _settings.Path;
            }

            List<string> list = new List<string>();
            list.Add(key + "=");
            list.Add("Path=" + path);
            list.Add("expires=Thu, 01 Jan 1970 00:00:01 GMT;");
            await SetCookie(string.Join(";", list));
        }

        public async Task<T?> GetValueAsync<T>(string key) where T : class
        {
            string text = await GetValueAsync(key);

            return JsonConvert.DeserializeObject<T>(text);
        }

        public async Task<string> GetValueAsync(string key)
        {
            string text = await GetCookie();

            string[] array = text.Split(new char[1] { ';' });
            foreach (string text2 in array)
            {
                if (!string.IsNullOrEmpty(text2) && text2.IndexOf('=') > 0 && text2.Substring(0, text2.IndexOf('=')).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return text2.Substring(text2.IndexOf('=') + 1);
                }
            }

            return "";
        }

        private async Task SetCookie(string value)
        {
            await JSRuntime.InvokeVoidAsync("eval", "document.cookie = '" + value + "'");
        }

        private async Task<string> GetCookie()
        {
            return await JSRuntimeExtensions.InvokeAsync<string>(JSRuntime, "eval", new object[1] { "document.cookie" });
        }

        private static string DateToUTC(TimeSpan span)
        {
            return DateTime.Now.Add(span).ToUniversalTime().ToString("R");
        }
    }
}
