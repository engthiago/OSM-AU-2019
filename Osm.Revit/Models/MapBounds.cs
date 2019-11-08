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

        public bool IsLarge(bool vertical)
        {
            var large = 0.01;
            var diff = vertical ? Top - Bottom : Right - Left;
            return diff > large;
        }

        public MapBounds[] Split(bool vertical)
        {
            var center = (Left + Right) / 2;
            var middle = (Bottom + Top) / 2;
            var eps = 0.00001;

            return new MapBounds[]
            {
                new MapBounds
                {
                    Left = Left,
                    Bottom = Bottom,
                    Right = vertical ? Right : center + eps,
                    Top = vertical ? middle + eps : Top
                },
                new MapBounds
                {
                    Left = vertical ? Left : center - eps,
                    Bottom = vertical ? middle - eps : Bottom,
                    Right = Right,
                    Top = Top
                }
            };
        }
    }
}
