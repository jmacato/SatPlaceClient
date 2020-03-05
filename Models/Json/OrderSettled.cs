using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class OrderSettled
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("paymentRequest")]
        public string Invoice { get; set; }

        [JsonProperty("pixelsPaintedCount")]
        public string TotalPixels { get; set; }

        [JsonProperty("sessionId")]
        public string SessionID { get; set; }
    }
}