using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Services;
using Autodesk.Revit.Attributes;
using System.Linq;
using Osm.Revit.Store;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class BuildingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            ContainerStore.Registration();
            var osmRepo = ContainerStore.Resolve<OsmRepository>();
            var buildingService = ContainerStore.Resolve<BuildingService>();
            var streetService = ContainerStore.Resolve<StreetService>();
            var topoService = ContainerStore.Resolve<TopoSurfaceService>();
            var viewService = ContainerStore.Resolve<View3DService>();

            using (Transaction t = new Transaction(doc, "Build City"))
            {
                t.Start();

                // -73.8513, 40.6796, -73.8386, 40.6890
                using (var source = osmRepo.GetMapStream(-74.0533, 40.6804, -73.9976, 40.7124))
                {
                    var everything = source.ToList();
                    buildingService.Run(doc, everything);
                    streetService.Run(doc, everything);
                    topoService.Run(doc);
                    viewService.Run(doc);
                }

                t.Commit();
            }


            return Result.Succeeded;
        }
    }
}
