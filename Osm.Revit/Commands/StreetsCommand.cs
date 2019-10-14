using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class StreetsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var httpService = new HttpService();
            var mapBounds = new MapBounds
            {
                Left = -73.85282,
                Bottom = 40.68363,
                Right = -73.84965,
                Top = 40.68585,
            };

            var source = httpService.GetMapStream(mapBounds);

            var streetWidth = UnitUtils.ConvertToInternalUnits(3000, DisplayUnitType.DUT_MILLIMETERS);

            var everything = source.Where(n => true).ToList();
            var streets = everything.Where(n => (n.Type == OsmGeoType.Way && 
                                                n.Tags != null 
                                                && n.Tags.Contains("highway", "residential")));

            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var streetShapeIds = new List<ElementId>();

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();

            using (Transaction t = new Transaction(doc, "Transfer OSM Streets"))
            {
                t.Start();
                var sketchPlane = SketchPlane.Create(doc, levelId);

                foreach (var street in streets)
                {
                    if (street is Way way)
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
                            XYZ direct = line.Direction;

                            //doc.Create.NewModelCurve(line, sketchPlane);

                            XYZ pp0 = line.GetEndPoint(0).Add(direct.CrossProduct(XYZ.BasisZ).Multiply(streetWidth));
                            XYZ pp1 = line.GetEndPoint(1).Add(direct.CrossProduct(XYZ.BasisZ).Multiply(streetWidth));

                            XYZ pn0 = line.GetEndPoint(0).Subtract(direct.CrossProduct(XYZ.BasisZ).Multiply(streetWidth));
                            XYZ pn1 = line.GetEndPoint(1).Subtract(direct.CrossProduct(XYZ.BasisZ).Multiply(streetWidth));

                            Line linep = Line.CreateBound(pp0, pp1);
                            Line linen = Line.CreateBound(pn1, pn0);

                            Line lineCap0 = Line.CreateBound(pp1, pn1);
                            Line lineCap1 = Line.CreateBound(pn0, pp0);

                            var curveLoop = new CurveLoop();
                            curveLoop.Append(linep);
                            curveLoop.Append(lineCap0);
                            curveLoop.Append(linen);
                            curveLoop.Append(lineCap1);

                            //doc.Create.NewModelCurve(linep, sketchPlane);
                            //doc.Create.NewModelCurve(linen, sketchPlane);
                            //doc.Create.NewModelCurve(lineCap0, sketchPlane);
                            //doc.Create.NewModelCurve(lineCap1, sketchPlane);

                            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, 1);
                            var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                            directShape.AppendShape(new List<GeometryObject> { solid });
                            streetShapeIds.Add(directShape.Id);
                        }
                    }
                }

                t.Commit();
            }

            var min = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Left).Subtract(XYZ.BasisZ.Multiply(100));
            var max = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Right).Add(XYZ.BasisZ.Multiply(200));
            var outLine = new Outline(min, max);
            var bbFilter = new BoundingBoxIntersectsFilter(outLine, true);

            var filteredShapes = new FilteredElementCollector(doc, streetShapeIds)
                                .OfCategory(BuiltInCategory.OST_GenericModel)
                                .WherePasses(bbFilter);

            using (Transaction t = new Transaction(doc, "Clean geometry"))
            {
                t.Start();

                doc.Delete(filteredShapes.Select(s => s.Id).ToList());

                t.Commit();
            }

            //bbFilter = new BoundingBoxIntersectsFilter(outLine);
            //filteredShapes = new FilteredElementCollector(doc, streetShapeIds)
            //                    .OfCategory(BuiltInCategory.OST_GenericModel)
            //                    .WherePasses(bbFilter);

            //var solidList = new List<Solid>();
            //foreach (var shape in filteredShapes)
            //{
            //    var solid = shape.get_Geometry(new Options()).FirstOrDefault(g => g is Solid) as Solid;
            //    solidList.Add(solid);
            //}

            //var bb0 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Left);
            //var bb1 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Left);
            //var bb2 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Right);
            //var bb3 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Right);

            //var l0 = Line.CreateBound(bb0, bb1);
            //var l1 = Line.CreateBound(bb1, bb2);
            //var l2 = Line.CreateBound(bb2, bb3);
            //var l3 = Line.CreateBound(bb3, bb0);

            //var curveLoop2 = new CurveLoop();
            //curveLoop2.Append(l0);
            //curveLoop2.Append(l1);
            //curveLoop2.Append(l2);
            //curveLoop2.Append(l3);

            //var bigSolid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop2 }, XYZ.BasisZ, 10);

            //Solid carvedSolid = bigSolid;
            //for (int i = 0; i < solidList.Count; i++)
            //{
            //    var solid = solidList[i];
            //    try
            //    {
            //        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(carvedSolid, solid, BooleanOperationsType.Difference);
            //    }
            //    catch (Exception e)
            //    {
            //        continue;
            //    }
            //}

            ////var difference = BooleanOperationsUtils.ExecuteBooleanOperation(bigSolid, carvedSolid, BooleanOperationsType.Difference);

            //using (Transaction t = new Transaction(doc, "Solids"))
            //{
            //    t.Start();
            //    var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            //    directShape.AppendShape(new List<GeometryObject> { carvedSolid });
            //    streetShapeIds.Add(directShape.Id);

            //    t.Commit();
            //}

            return Result.Succeeded;
        }
    }
}
