using di.container;
using Osm.Revit.Services;

namespace Osm.Revit.Store
{
    public static class ContainerStore
    {
        private static bool registered;

        public static void Registration()
        {
            if (registered) return;

            Container.Register<OsmStore>();
            Container.Register<CoordinatesService>();
            Container.Register<GeometryService>();
            Container.Register<OsmRepository>();
            Container.Register<StreetService>();
            Container.Register<SolidGeometryService>();
            Container.Register<BuildingService>();
            Container.Register<TopoSurfaceService>();
            Container.Register<View3DService>();

            registered = true;
        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
