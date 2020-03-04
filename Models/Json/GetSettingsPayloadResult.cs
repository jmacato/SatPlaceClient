using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class GetSettingsPayloadResult
    {
        [JsonProperty("data")]
        public RawOrderSettingsResult RawData { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
