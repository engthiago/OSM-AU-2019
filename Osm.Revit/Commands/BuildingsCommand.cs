using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp.Streams;
using System.IO;
using OsmSharp.Tags;
using Autodesk.Revit.Attributes;
using System.Linq;
using OsmSharp;
using System.Collections.Generic;
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

            using (Transaction t = new Transaction(doc, "Build Streets"))
            {
                t.Start();

                using (var source = osmRepo.GetMapStream(-73.85282, 40.68363, -73.84965, 40.68585))
                {
                    var everthing = source.ToList();
                    buildingService.Run(doc, everthing);
                }

                t.Commit();
            }


            return Result.Succeeded;
        }
    }
}
