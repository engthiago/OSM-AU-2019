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
            var osmServie = ContainerStore.Resolve<OsmService>();

            using (Transaction t = new Transaction(doc, "Build City"))
            {
                t.Start();

                // -73.8513, 40.6796, -73.8386, 40.6890
                // -74.0533, 40.6804, -73.9976, 40.7124
                osmServie.Run(doc,
                    new Models.MapBounds
                    {
                        Left = -73.8513, Bottom = 40.6796, Right = -73.8386, Top = 40.6890
                    });

                t.Commit();
            }


            return Result.Succeeded;
        }
    }
}
