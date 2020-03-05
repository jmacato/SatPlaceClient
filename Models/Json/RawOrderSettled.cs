using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class RawOrderSettled
    {
        [JsonProperty("data")]
        public OrderSettled Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
