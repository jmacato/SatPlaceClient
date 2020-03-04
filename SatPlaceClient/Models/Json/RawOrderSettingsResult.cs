using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class RawOrderSettingsResult
    {
        [JsonProperty("boardLength")]
        public uint BoardLength { get; set; }

        [JsonProperty("colors")]
        public string[] Colors { get; set; }

        [JsonProperty("invoiceExpiry")]
        public int InvoiceExpiry { get; set; }

        [JsonProperty("orderPixelsLimit")]
        public int OrderPixelsLimit { get; set; }
        
        [JsonProperty("pricePerPixel")]
        public int PricePerPixel { get; set; }
    }
}
