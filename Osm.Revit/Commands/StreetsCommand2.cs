using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class StreetsCommand2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var streetWidth = UnitUtils.ConvertToInternalUnits(3000, DisplayUnitType.DUT_MILLIMETERS);
            var interSectionSetBack = streetWidth / 2;


            var httpService = new HttpService();
            var mapBounds = new MapBounds
            {
                Left = -73.85282,
                Bottom = 40.68363,
                Right = -73.84965,
                Top = 40.68585,
            };

            var source = httpService.GetMapStream(mapBounds);

            var everything = source.Where(n => true).ToList();
            var streets = everything.Where(n => (n.Type == OsmGeoType.Way &&
                                                n.Tags != null
                                                && n.Tags.Contains("highway", "residential")
                                                && n is Way)).Cast<Way>();

            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();
            var lines = new List<Line>();

            var geometryService = new GeometryService();
            var boundLines = geometryService.CreateBoundingLines(mapBounds);

            foreach (var street in streets)
            {
                var points = new List<XYZ>();
                foreach (var nodeId in street.Nodes)
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
                    var start = points[i];
                    var end = points[i + 1];

                    if (!geometryService.IsInsideBounds(boundLines, start) && !geometryService.IsInsideBounds(boundLines, end)) continue;

                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    lines.Add(line);
                }
            }


            using (Transaction t = new Transaction(doc, "Lines"))
            {
                t.Start();

                var sketchplane = SketchPlane.Create(doc, levelId);

                foreach (var curve in boundLines)
                {
                    doc.Create.NewModelCurve(curve, sketchplane);
                }

                t.Commit();
            }

            if (lines.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Lines"))
                {
                    t.Start();

                    var sketchplane = SketchPlane.Create(doc, levelId);
                    foreach (var line in lines)
                    {
                        doc.Create.NewModelCurve(line, sketchplane);
                    }

                    t.Commit();
                }
            }

            var intersPoints = geometryService.GetIntersections(lines, 4);
            foreach (var point in intersPoints)
            {

            }



            return Result.Succeeded;
        }
    }
}
