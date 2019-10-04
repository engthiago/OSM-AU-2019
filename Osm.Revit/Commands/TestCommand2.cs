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
using System;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var mapBounds = new MapBounds
            {
                Left = -73.88025,
                Bottom = 40.71562,
                Right = -73.87901,
                Top = 40.71712,
            };

            var httpService = new HttpService();
            var source = httpService.GetMapStream(mapBounds);

            var everything = source.FilterNodes(n => true).ToList();
            var buildings = everything.Where(n => n.Tags.IsTrue("building"));

            System.Windows.MessageBox.Show(buildings.Count().ToString());
        
            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var points = new List<Tuple<double, double>>
            {
                new Tuple<double, double>(40.7168228, -73.8794354),
                new Tuple<double, double>(40.7168317, -73.8792807),
                new Tuple<double, double>(40.7168777, -73.8794408),
                new Tuple<double, double>(40.7168866, -73.8792861),
                                new Tuple<double, double>(40.7168228, -73.8794354),
            };

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();

            using (Transaction t = new Transaction(doc, "points"))
            {
                t.Start();
                var sketchPlane = SketchPlane.Create(doc, levelId);

                List<XYZ> pps = new List<XYZ>();
                foreach (var item in points)
                {
                    pps.Add(coordService.GetRevitCoords(item.Item1, item.Item2));
                }

                for (int i = 0; i < pps.Count - 1; i++)
                {
                    Line line = Line.CreateBound(pps[i], pps[i + 1]);
                    doc.Create.NewModelCurve(line, sketchPlane);
                }
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
