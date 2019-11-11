using Newtonsoft.Json;
using System.Collections.Generic;

namespace Osm.Web.Models
{
    public partial class ModelDerivativePayload
    {
        [JsonProperty("input")]
        public Input Input { get; set; }

        [JsonProperty("output")]
        public Output Output { get; set; }
    }

    public partial class Input
    {
        [JsonProperty("urn")]
        public string Urn { get; set; }
    }

    public partial class Output
    {
        [JsonProperty("formats")]
        public List<Format> Formats { get; set; }
    }

    public partial class Format
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("views")]
        public List<string> Views { get; set; }
    }
}
