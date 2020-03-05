using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class RawNewOrder
    {
        [JsonProperty("data")]
        public NewOrder Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
