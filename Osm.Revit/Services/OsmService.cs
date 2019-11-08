using Autodesk.Revit.DB;
using Osm.Revit.Models;
using Osm.Revit.Store;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit.Services
{
    public class OsmService
    {
        private readonly BuildingService buildingService;
        private readonly StreetService streetService;
        private readonly TopoSurfaceService topoService;
        private readonly View3DService viewService;

        public OsmService(BuildingService buildingService,
            StreetService streetService, TopoSurfaceService topoService, View3DService viewService)
        {
            this.buildingService = buildingService;
            this.streetService = streetService;
            this.topoService = topoService;
            this.viewService = viewService;
        }

        public void Run(Document doc, MapBounds mapbounds)
        {
            var streamService = ContainerStore.Resolve<IMapStreamService>();
            var everything = streamService.GetOsmGeoList(mapbounds);

            buildingService.Run(doc, everything);
            streetService.Run(doc, everything);
            topoService.Run(doc);
            viewService.Run(doc);
        }

    }
}
