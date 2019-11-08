using di.container;
using Osm.Revit.Services;

namespace Osm.Revit.Store
{
    public static class ContainerStore
    {
        private static bool registered;

        public static void Registration<MapStreamService>()
        {
            if (registered) return;

            Container.Register<OsmStore>();
            Container.Register<OsmService>();
            Container.Register<CoordinatesService>();
            Container.Register<GeometryService>();
            Container.Register<StreetService>();
            Container.Register<SolidGeometryService>();
            Container.Register<BuildingService>();
            Container.Register<TopoSurfaceService>();
            Container.Register<View3DService>();
            Container.Register<IMapStreamService, MapStreamService>();

            registered = true;
        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
