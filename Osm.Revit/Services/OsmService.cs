using Autodesk.Revit.DB;
using Osm.Revit.Models;
using Osm.Revit.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Run(Document doc, MapBounds mapbounds )
        {
            using (var source = osmRepo.GetMapStream(mapbounds))
            {
                var everything = source.ToList();
                buildingService.Run(doc, everything);
                streetService.Run(doc, everything);
                topoService.Run(doc);
                viewService.Run(doc);
            }
        }
    }
}
