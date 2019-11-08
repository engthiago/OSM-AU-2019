using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Osm.Revit.Models;
using OsmSharp;
using OsmSharp.Streams;


namespace Osm.Revit.Services
{
    public class MapStreamHttpService : IMapStreamService
    {
        private readonly HttpClient Client;
        private readonly string BaseAddress = "https://api.openstreetmap.org/api/0.6/";

        public MapStreamHttpService()
        {
            this.Client = new HttpClient();
        }

        public List<OsmGeo> GetOsmGeoList(MapBounds mapbounds)
        {
            var task = this.Client
                .GetStreamAsync($"{BaseAddress}/map?bbox={mapbounds.Left},{mapbounds.Bottom},{mapbounds.Right},{mapbounds.Top}");
            using (var stream = new XmlOsmStreamSource(task.Result))
            {
                return stream.ToList();
            }
        }
    }
}
