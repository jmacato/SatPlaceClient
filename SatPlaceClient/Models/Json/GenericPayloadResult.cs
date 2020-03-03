using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class GenericPayloadResult
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
