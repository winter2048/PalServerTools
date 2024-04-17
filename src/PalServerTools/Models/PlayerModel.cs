using Newtonsoft.Json;
using System.Numerics;

namespace PalServerTools.Models
{
    public class PlayerModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("playerid")]
        public string? PlayerID { get; set; }

        [JsonProperty("userid")]
        public string SteamID { get; set; }

        [JsonProperty("ip")]
        public string IpAddress { get; set; }
   
        [JsonProperty("ping")]
        public float Ping { get; set; }

        [JsonProperty("level")]
        public float Level { get; set; }

        [JsonProperty("location_x")]
        public float Location_x { get; set; }
    
        [JsonProperty("location_y")]
        public float Location_y { get; set; }
    }

    public class PlayersModel 
    {
        [JsonProperty("players")]
        public List<PlayerModel> Players { get; set; }
    }
}
