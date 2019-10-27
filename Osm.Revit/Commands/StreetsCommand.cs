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
    public class StreetsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var streetWidth = UnitUtils.ConvertToInternalUnits(6000, DisplayUnitType.DUT_MILLIMETERS);
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

                var intersectShapes = new List<DirectShape>();
                foreach (var intersec in intersections)
                {
                    foreach (var lin in intersec.CurveLoop)
                    {
                        doc.Create.NewModelCurve(lin, sketchplane);
                    }
                    var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { intersec.CurveLoop }, XYZ.BasisZ, 0.1);
                    var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                    directShape.AppendShape(new List<GeometryObject> { solid });
                    intersectShapes.Add(directShape);
                }


                foreach (var streetSegment in streetSegments)
                {
                    var start = streetSegment.Line.GetEndPoint(0);
                    var end = streetSegment.Line.GetEndPoint(1);
                    var mid = streetSegment.Line.Evaluate(0.5, true);

                    var startIntersect = intersections.FirstOrDefault(o => o.Origin.IsAlmostEqualTo(start));
                    var endIntersect = intersections.FirstOrDefault(o => o.Origin.IsAlmostEqualTo(end));

                    var newStart = streetService.ProjectLineToIntersection(streetSegment.Line, startIntersect);
                    var newEnd = streetService.ProjectLineToIntersection(streetSegment.Line, endIntersect);

                    newStart = newStart ?? start;
                    newEnd = newEnd ?? end;

                    var line = Line.CreateBound(newStart, newEnd);
                    //doc.Create.NewModelCurve(line, sketchplane);

                    var lineOff0 = line.CreateOffset(streetWidth / 2, XYZ.BasisZ);
                    var lineOff1 = line.CreateOffset(streetWidth / 2, -XYZ.BasisZ);

                    var curveLoop = lineOff0.CreateCurveLoop(lineOff1);
                    var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, 0.1);
                    var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Roads));
                    directShape.AppendShape(new List<GeometryObject> { solid });
                    intersectShapes.Add(directShape);

                }

                t.Commit();
            }



            return Result.Succeeded;
        }
    }
}
