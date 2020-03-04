using Newtonsoft.Json; 

namespace SatPlaceClient.Models.Json
{
    public class CanvasPixel
    {
        [JsonProperty("coordinates")]
        public uint[] Coordinates { get; set; }

        [JsonProperty("color")]
        public string HexColor { get; set; }
    }
}
