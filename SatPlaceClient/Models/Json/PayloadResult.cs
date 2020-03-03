using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class PayloadResult
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
