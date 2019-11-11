using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Models
{
    public class DataManagementSignedUrl
    {
        [JsonProperty("signedUrl")]
        public string SignedUrl { get; set; }

        [JsonProperty("expiration")]
        public long Expiration { get; set; }

        [JsonProperty("singleUse")]
        public bool SingleUse { get; set; }
    }
}
