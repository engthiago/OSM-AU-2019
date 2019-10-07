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
            var mapBounds = new MapBounds
            {
                Left = 140.78583,
                Bottom = -37.83315,
                Right = 140.78851,
                Top = -37.83048,
            };

            var source = httpService.GetMapStream(mapBounds);

            var everything = source.FilterNodes(n => true).ToList();
            var buildings = everything.Where(n => n.Tags.IsTrue("building"));

            System.Windows.MessageBox.Show(buildings.Count().ToString());
        
            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();

            using (Transaction t = new Transaction(doc, "points"))
            {
                t.Start();
                var sketchPlane = SketchPlane.Create(doc, levelId);
                foreach (var building in buildings)
                {
                    if (building is Way way)
                    {
                        var points = new List<XYZ>();

                        foreach (var nodeId in way.Nodes)
                        {
                            var geometry = everything.FirstOrDefault(n => n.Id == nodeId);
                            if (geometry is Node node)
                            {
                                var coords = coordService.GetRevitCoords((double)node.Latitude, (double)node.Longitude);
                                points.Add(coords);
                            }
                        }

                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            Line line = Line.CreateBound(points[i], points[i + 1]);
                            doc.Create.NewModelCurve(line, sketchPlane);
                        }
                    }
                }

                var bb0 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Left);
                var bb1 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Left);
                var bb2 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Right);
                var bb3 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Right);

                var l0 = Line.CreateBound(bb0, bb1);
                var l1 = Line.CreateBound(bb1, bb2);
                var l2 = Line.CreateBound(bb2, bb3);
                var l3 = Line.CreateBound(bb3, bb0);

                doc.Create.NewModelCurve(l0, sketchPlane);
                doc.Create.NewModelCurve(l1, sketchPlane);
                doc.Create.NewModelCurve(l2, sketchPlane);
                doc.Create.NewModelCurve(l3, sketchPlane);

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
