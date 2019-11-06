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

        private static View3D Get3DView(Document doc) => doc.ActiveView as View3D ?? new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Views)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .FirstOrDefault(v => v.IsTemplate == false && v.IsPerspective == false);

        private static ElementId GetViewFamilyTypeId(Document doc) => new FilteredElementCollector(doc)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .FirstOrDefault<ViewFamilyType>(x => ViewFamily.ThreeDimensional == x.ViewFamily).Id;

        private static View3D GetOrCreate3DView(Document doc)
        {
            var view3D = Get3DView(doc) ?? View3D.CreateIsometric(doc, GetViewFamilyTypeId(doc));
            view3D.Name = "OsmDemoView";

            var settings = new FilteredElementCollector(doc).OfClass(typeof(StartingViewSettings)).Single() as StartingViewSettings;
            settings.ViewId = view3D.Id;

            return view3D;
        }

        public View3D Run(Document doc)
        {
            var view3d = GetOrCreate3DView(doc);

            view3d.IsSectionBoxActive = true;

            var height = UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_METERS);
            var groundDepth = UnitUtils.ConvertToInternalUnits(30, DisplayUnitType.DUT_METERS);

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
