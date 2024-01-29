namespace PalServerTools.Utils
{
    public interface ICookieUtil
    {
        Task SetValueAsync(string key, object value, TimeSpan? span = null, string path = null, string domain = null, bool? secure = null);

        Task SetValueAsync(string key, string value, TimeSpan? span = null, string path = null, string domain = null, bool? secure = null);

        Task<string> GetValueAsync(string key);

        Task<T> GetValueAsync<T>(string key) where T : class;

        Task RemoveAsync(string key, string path = null);
    }
}
