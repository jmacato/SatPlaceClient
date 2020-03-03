using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class GetSettingsPayloadResult
    {
        [JsonProperty("data")]
        public RawSettingsResult RawData { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
