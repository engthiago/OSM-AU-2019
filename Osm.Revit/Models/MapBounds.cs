using Newtonsoft.Json;

namespace Osm.Revit.Models
{
    public class MapBounds
    {
        [JsonProperty(PropertyName = "left")]
        public double Left { get; set; }
        [JsonProperty(PropertyName = "top")]
        public double Top { get; set; }
        [JsonProperty(PropertyName = "right")]
        public double Right { get; set; }
        [JsonProperty(PropertyName = "bottom")]
        public double Bottom { get; set; }
        public static MapBounds Deserialize(string json) => JsonConvert.DeserializeObject<MapBounds>(json);
        public static string Serialize(MapBounds mapBounds) => JsonConvert.SerializeObject(mapBounds);
    }
}
