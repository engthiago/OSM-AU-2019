using Autodesk.Revit.DB;
using Osm.Revit.Models;
using OsmSharp.Streams;
using System.Linq;

namespace Osm.Revit.Services
{
    public class OsmService
    {
        private readonly OsmRepository osmRepo;
        private readonly BuildingService buildingService;
        private readonly StreetService streetService;
        private readonly TopoSurfaceService topoService;
        private readonly View3DService viewService;

        public OsmService(OsmRepository osmRepo, BuildingService buildingService, 
            StreetService streetService, TopoSurfaceService topoService, View3DService viewService)
        {
            this.osmRepo = osmRepo;
            this.buildingService = buildingService;
            this.streetService = streetService;
            this.topoService = topoService;
            this.viewService = viewService;
        }

        public void Run(Document doc, MapBounds mapbounds)
        {
            using (var stream = osmRepo.GetMapStream(mapbounds))
            {
                RunWithStream(doc, stream);
            }
        }
        public void RunWithStream(Document doc, XmlOsmStreamSource stream)
        {
            var everything = stream.ToList();
            buildingService.Run(doc, everything);
            streetService.Run(doc, everything);
            topoService.Run(doc);
            viewService.Run(doc);
        }

    }
}
