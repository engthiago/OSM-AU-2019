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
            var streetService = ContainerStore.Resolve<StreetService>();
            var topoService = ContainerStore.Resolve<TopoSurfaceService>();

            using (Transaction t = new Transaction(doc, "Build City"))
            {
                t.Start();

                using (var source = osmRepo.GetMapStream())
                {
                    var everything = source.ToList();
                    buildingService.Run(doc, everything);
                    streetService.Run(doc, everything);
                    topoService.Run(doc);
                }

                t.Commit();
            }


            return Result.Succeeded;
        }
    }
}
