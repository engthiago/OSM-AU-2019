using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Models.Osm;
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

            var everything = source.ToList();
            var nodes = everything.Where(n => n.Type == OsmGeoType.Node).Cast<Node>().ToList();
            var osmStreets = everything.Where(n => (n.Type == OsmGeoType.Way &&
                                                n.Tags != null
                                                && n.Tags.Contains("highway", "residential")
                                                && n is Way)).Cast<Way>().ToList();

            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();
            var lines = new List<Line>();

            var geometryService = new GeometryService();
            var boundLines = geometryService.CreateBoundingLines(mapBounds);

            var streetService = new StreetService();
            var streetSegments = streetService.CreateStreetSegments(osmStreets, nodes, boundLines, mapBounds);
            var intersections = streetService.CreateIntersections(streetSegments);


            using (Transaction t = new Transaction(doc, "Lines"))
            {
                t.Start();

                var sketchplane = SketchPlane.Create(doc, levelId);

                foreach (var curve in boundLines)
                {
                    doc.Create.NewModelCurve(curve, sketchplane);
                }

                foreach (var line in streetSegments.Select(s => s.Line))
                {
                    doc.Create.NewModelCurve(line, sketchplane);
                }

                foreach (var intersec in intersections)
                {
                    var arc = Arc.Create(intersec.Origin, 20, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY);
                    doc.Create.NewModelCurve(arc, sketchplane);
                }

                foreach (var intersec in intersections)
                {
                    var intersStreetSegments = streetSegments.Where(s => intersec.StreetSegmentIds.Contains(s.SegmentId));
                    var arc = Arc.Create(intersec.Origin, 20, 0, Math.PI * 1.99, XYZ.BasisX, XYZ.BasisY);

                    var arcDic = new Dictionary<StreetSegment, double>();
                    foreach (var segm in intersStreetSegments)
                    {
                        arc.Intersect(segm.Line, out IntersectionResultArray resultArray);
                        if (resultArray != null && !resultArray.IsEmpty)
                        {
                            var result = resultArray.get_Item(0);
                            var intPoint = result.XYZPoint;
                            var pResult = arc.Project(intPoint);
                            var param = arc.ComputeNormalizedParameter(pResult.Parameter);
                            arcDic.Add(segm, param);
                        }
                    }

                    var orderedDic = arcDic.OrderBy(d => d.Value);

                    foreach (var intt in orderedDic)
                    {
                        var point = arc.Evaluate(intt.Value, true);
                        var arc2 = Arc.Create(point, 5, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY);
                        doc.Create.NewModelCurve(arc2, sketchplane);
                    }
                }

                t.Commit();
            }



            return Result.Succeeded;
        }
    }
}
