using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Osm.Revit.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class TopoSurfaceService
    {
        private readonly CoordinatesService coordService;
        private readonly OsmStore osmStore;

        public TopoSurfaceService(CoordinatesService coordService, OsmStore osmStore)
        {
            this.coordService = coordService;
            this.osmStore = osmStore;
        }

        public TopographySurface Run(Document doc) 
        {
            var bb0 = coordService.GetRevitCoords(osmStore.MapBottom, osmStore.MapLeft);
            var bb1 = coordService.GetRevitCoords(osmStore.MapTop, osmStore.MapLeft);
            var bb2 = coordService.GetRevitCoords(osmStore.MapTop, osmStore.MapRight);
            var bb3 = coordService.GetRevitCoords(osmStore.MapBottom, osmStore.MapRight);

            var points = new List<XYZ>()
            {
                bb0, bb1, bb2, bb3
            };

            var topography = TopographySurface.Create(doc, points);
            return topography;
        }


    }
}
