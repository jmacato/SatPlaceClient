using Newtonsoft.Json; 

namespace SatPlaceClient.Models.Json
{
    public class PixelResult
    {
        [JsonProperty("data")]
        public string DataBase64 { get; set; }
    }
}
