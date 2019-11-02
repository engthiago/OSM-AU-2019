using Osm.Revit.Models;
using Osm.Revit.Store;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class OsmRepository
    {
        private readonly HttpClient client;
        private readonly string baseAddres = "https://api.openstreetmap.org/api/0.6/";
        private readonly OsmStore osmStore;

        public OsmRepository(OsmStore osmStore)
        {
            this.client = new HttpClient();
            this.osmStore = osmStore;
        }

        public XmlOsmStreamSource GetMapStream(double left = -73.85282, double bottom = 40.68363, double right = -73.84965, double top = 40.68585)
        {
            var mapBounds = new MapBounds
            {
                Left = left,
                Bottom = bottom,
                Right = right,
                Top = top,
            };

            return GetMapStream(mapBounds);
        }

        public XmlOsmStreamSource GetMapStream(MapBounds mapBounds)
        {
            osmStore.Geolocate(mapBounds);
            var task = this.client
                .GetStreamAsync($"{baseAddres}/map?bbox={osmStore.MapLeft},{osmStore.MapBottom},{osmStore.MapRight},{osmStore.MapTop}");
            var source = new XmlOsmStreamSource(task.Result);
            return source;
        }
    }
}
