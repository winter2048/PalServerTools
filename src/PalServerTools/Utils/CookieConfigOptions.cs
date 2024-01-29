namespace PalServerTools.Utils
{
    public class CookieConfigOptions
    {
        public TimeSpan DefaultExpire { get; set; } = TimeSpan.Zero;


        public string Path { get; set; } = "/";


        public string Domain { get; set; } = string.Empty;


        public bool IsSecure { get; set; }
    }
}
