using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp.Streams;
using System.IO;
using OsmSharp.Tags;
using Autodesk.Revit.Attributes;
using System.Linq;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var httpService = new HttpService();
            var source = httpService.GetMapStream(new MapBounds
            {
                Left = -74.06062f,
                Bottom = 40.79735f,
                Right = -74.05220f,
                Top = 40.80097f,
            });

            var everything = source.FilterNodes(n => true).ToList();
            var buildings = everything.Where(n => n.Tags.IsTrue("buildings")).ToList();

            return Result.Succeeded;
        }
    }
}
