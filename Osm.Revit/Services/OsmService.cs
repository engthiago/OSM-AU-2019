using Autodesk.Revit.DB;
using Osm.Revit.Models;
using Osm.Revit.Store;
using System.Collections.Generic;
using System.Linq;
using OsmSharp;

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

        private List<OsmGeo> GetOsmGeoListFromMapStreamService(MapBounds mapbounds)
        {
            List<OsmGeo> list = new List<OsmGeo>();

            if (mapbounds.IsLarge(false))
            {
                var split = mapbounds.Split(false);
                list.AddRange(GetOsmGeoListFromMapStreamService(split[0]));
                list.AddRange(GetOsmGeoListFromMapStreamService(split[1]));
            }
            else if (mapbounds.IsLarge(true))
            {
                var split = mapbounds.Split(true);
                list.AddRange(GetOsmGeoListFromMapStreamService(split[0]));
                list.AddRange(GetOsmGeoListFromMapStreamService(split[1]));
            }
            else
            {
                var streamService = ContainerStore.Resolve<IMapStreamService>();
                return streamService.GetOsmGeoList(mapbounds);
            }

            return list.GroupBy(n => n.Id).Select(g => g.First()).ToList();
        }

        private List<OsmGeo> GetOsmGeoListStoreBounds()
        {
            return GetOsmGeoListFromMapStreamService(OsmStore.Bounds);
        }

        public void Run(Document doc)
        {
            var everything = GetOsmGeoListStoreBounds();
            buildingService.Run(doc, everything);
            streetService.Run(doc, everything);
            topoService.Run(doc);
            viewService.Run(doc);
        }

    }
}
