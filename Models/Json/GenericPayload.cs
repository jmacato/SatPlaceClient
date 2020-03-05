using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class GenericPayload
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
