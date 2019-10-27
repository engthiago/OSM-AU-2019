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

                //foreach (var intersec in intersections)
                //{
                //    var arc = Arc.Create(intersec.Origin, 20, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY);
                //    doc.Create.NewModelCurve(arc, sketchplane);
                //}

                foreach (var intersec in intersections)
                {
                    var intersStreetSegments = streetSegments.Where(s => intersec.StreetSegmentIds.Contains(s.SegmentId));
                    var arc = Arc.Create(intersec.Origin, 20, 0, Math.PI * 1.99, XYZ.BasisX, XYZ.BasisY);

                    var lineOffsetDic = new Dictionary<Line, double>();
                    var offsetLines = new List<Line>();

                    foreach (var segm in intersStreetSegments)
                    {
                        var line0 = segm.Line.CreateOffset(streetWidth / 2, XYZ.BasisZ) as Line;
                        offsetLines.Add(line0);
                        var line1 = segm.Line.CreateOffset(streetWidth / 2, -XYZ.BasisZ) as Line;
                        offsetLines.Add(line1);
                    }

                    foreach (var line in offsetLines)
                    {
                        arc.Intersect(line, out IntersectionResultArray resultArray);
                        if (resultArray != null && !resultArray.IsEmpty)
                        {
                            var result = resultArray.get_Item(0);
                            var intPoint = result.XYZPoint;
                            var pResult = arc.Project(intPoint);
                            var param = arc.ComputeNormalizedParameter(pResult.Parameter);
                            lineOffsetDic.Add(line, param);
                        }
                    }

                    var orderedDic = lineOffsetDic.OrderBy(d => d.Value).ToList();
                    orderedDic.Add(orderedDic[0]);

                    var textTypeId = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).FirstElementId();

                    //var current = 1;
                    foreach (var intt in orderedDic)
                    {
                        var point = arc.Evaluate(intt.Value, true);
                        var arc2 = Arc.Create(point, 5, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY);
                        doc.Create.NewModelCurve(arc2, sketchplane);
                        //TextNote.Create(doc, doc.ActiveView.Id, point, current.ToString(), textTypeId);
                        //current++;
                    }

                    if (orderedDic.Count < 9)
                    {
                        continue;
                    }

                    var intersectLoop = new List<Line>();
                    var current = 1;
                    for (int i = 0; i < orderedDic.Count() - 2; i += 2)
                    {
                        var line1 = orderedDic[i].Key.Clone();
                        var line2 = orderedDic[i + 1].Key.Clone();
                        var line3 = orderedDic[i + 2].Key.Clone();

                        line2.Intersect(line3, out IntersectionResultArray resultArray);
                        if (resultArray != null && !resultArray.IsEmpty)
                        {
                            var result = resultArray.get_Item(0);
                            var end = result.XYZPoint;
                            var start = line1.Project(end).XYZPoint;
                            var ll = Line.CreateBound(start, end);
                            intersectLoop.Add(ll);
                            var mid = ll.Evaluate(0.5, true);
                            TextNote.Create(doc, doc.ActiveView.Id, mid, current.ToString(), textTypeId);
                            current++;
                        }
                    }

                    foreach (var line in intersectLoop)
                    {
                        doc.Create.NewModelCurve(line, sketchplane);
                    }
                }

                t.Commit();
            }



            return Result.Succeeded;
        }
    }
}
