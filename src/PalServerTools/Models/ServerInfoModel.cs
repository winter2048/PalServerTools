using Newtonsoft.Json;

namespace PalServerTools.Models
{
    public class ServerInfoModel
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("servername")]
        public string ServerName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }


        public override string ToString() => $"[{ServerName}][{Version}][{Description}]";
    }
}
