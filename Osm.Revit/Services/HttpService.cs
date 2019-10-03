using Osm.Revit.Models;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class HttpService
    {
        private readonly HttpClient client;
        private readonly string baseAddres = "https://api.openstreetmap.org/api/0.6/"; 

        public HttpService()
        {
            this.client = new HttpClient();
        }

        public XmlOsmStreamSource GetMapStream(MapBounds mapBounds)
        {
            var task = this.client.GetStreamAsync($"https://api.openstreetmap.org/api/0.6/map?bbox={mapBounds.Left},{mapBounds.Bottom},{mapBounds.Right},{mapBounds.Top}");
            var source = new XmlOsmStreamSource(task.Result);
            return source;
        }
    }
}
