using Autodesk.Revit.DB;
using Osm.Revit.Store;
using System.Linq;

namespace Osm.Revit.Services
{
    public class View3DService
    {
        private readonly CoordinatesService coordService;
        private readonly OsmStore osmStore;

        public View3DService(CoordinatesService coordService, OsmStore osmStore)
        {
            this.coordService = coordService;
            this.osmStore = osmStore;
        }

        public View3D Run(Document doc)
        {
            var view3d = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => v.IsTemplate == false && v.IsPerspective == false);

            if (view3d == null) return null;

            view3d.IsSectionBoxActive = true;

            var height = UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_METERS);
            var groundDepth = UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_METERS);

            var max = this.coordService
                .GetRevitCoords(osmStore.MapTop, osmStore.MapRight)
                .Add(new XYZ(0, 0, height));

            var min = this.coordService
                .GetRevitCoords(osmStore.MapBottom, osmStore.MapLeft)
                .Subtract(new XYZ(0, 0, groundDepth));

            var bbXYZ = new BoundingBoxXYZ
            {
                Enabled = true,
                Max = max,
                Min = min
            };

            view3d.SetSectionBox(bbXYZ);

            view3d.SetCategoryHidden(new ElementId(BuiltInCategory.OST_SectionBox), true);

            return view3d;
        }
    }
}
