using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class NewOrder
    {
        [JsonProperty("paymentRequest")]
        public string Invoice { get; set; }
    }
}