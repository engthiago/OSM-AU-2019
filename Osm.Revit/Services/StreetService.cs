using Autodesk.Revit.DB;
using Osm.Revit.Models.Osm;
using Osm.Revit.Store;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit.Services
{
    public class StreetService
    {
        private readonly GeometryService geometryService;
        private readonly CoordinatesService coordService;
        private readonly OsmStore osmStore;
        private readonly SolidGeometryService solidGeometryService;

        public StreetService(GeometryService geometryService, CoordinatesService coordService, OsmStore osmStore, SolidGeometryService solidGeometryService)
        {
            this.geometryService = geometryService;
            this.coordService = coordService;
            this.osmStore = osmStore;
            this.solidGeometryService = solidGeometryService;
        }

        public List<DirectShape> Run(Document doc, List<OsmGeo> everything)
        {
            List<DirectShape> streetsAndIntersections = new List<DirectShape>();

            var osmStreets = everything.Where(n => (n.Type == OsmGeoType.Way &&
                                    n.Tags != null
                                    && n.Tags.Contains("highway", "residential")
                                    && n is Way)).Cast<Way>().ToList();

            var streetDataList = CreateStreetSegments(osmStreets, everything);
            var intersectionDataList = CreateIntersections(streetDataList);

            foreach (var intersData in intersectionDataList)
            {
                var intersection = solidGeometryService
                            .Build(doc, new List<CurveLoop> { intersData.CurveLoop }, osmStore.DefaultStreetThickness, new ElementId(BuiltInCategory.OST_Roads));

                streetsAndIntersections.Add(intersection);
            }

            foreach (var streetData in streetDataList)
            {
                var start = streetData.Line.GetEndPoint(0);
                var end = streetData.Line.GetEndPoint(1);
                var mid = streetData.Line.Evaluate(0.5, true);

                var startIntersect = intersectionDataList.FirstOrDefault(o => o.Origin.IsAlmostEqualTo(start));
                var endIntersect = intersectionDataList.FirstOrDefault(o => o.Origin.IsAlmostEqualTo(end));

                var newStart = ProjectLineToIntersection(streetData.Line, startIntersect);
                var newEnd = ProjectLineToIntersection(streetData.Line, endIntersect);

                newStart = newStart ?? start;
                newEnd = newEnd ?? end;

                var line = Line.CreateBound(newStart, newEnd);

                var lineOff0 = line.CreateOffset(osmStore.DefaultStreetWidth / 2, XYZ.BasisZ);
                var lineOff1 = line.CreateOffset(osmStore.DefaultStreetWidth / 2, -XYZ.BasisZ);

                var curveLoop = lineOff0.CreateCurveLoop(lineOff1);

                var street = solidGeometryService
                        .Build(doc, new List<CurveLoop> { curveLoop }, osmStore.DefaultStreetThickness, new ElementId(BuiltInCategory.OST_Roads));

                streetsAndIntersections.Add(street);
            }

            return streetsAndIntersections;
        }

        public List<StreetSegment> CreateStreetSegments(List<Way> OsmStreets, List<OsmGeo> everything)
        {
            var boundLines = this.geometryService.CreateBoundingLines();
            List<StreetSegment> streetSegments = new List<StreetSegment>();

            foreach (var osmStreet in OsmStreets)
            {
                var points = new List<XYZ>();
                foreach (var nodeId in osmStreet.Nodes)
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

                    if (!geometryService.IsInsideBounds(boundLines, start) && 
                        !geometryService.IsInsideBounds(boundLines, end)) continue;

                    var line = Line.CreateBound(points[i], points[i + 1]);
                    var segment = new StreetSegment();
                    segment.Id = (long)osmStreet.Id;
                    segment.SegmentId = osmStore.MoveNextId();
                    segment.Line = line;
                    segment.Width = osmStore.DefaultStreetWidth;
                    if (osmStreet.Tags.TryGetValue("name", out string name))
                    {
                        segment.Name = name;
                    }

                    streetSegments.Add(segment);
                }
            }

            return streetSegments;
        }

        public List<StreetIntersection> CreateIntersections(List<StreetSegment> streetSegments)
        {
            var uniquePoints = new List<XYZ>();
            foreach (var line in streetSegments.Select(s => s.Line))
            {
                var start = line.GetEndPoint(0);
                var end = line.GetEndPoint(1);

                if (!geometryService.ListContainsPoint(uniquePoints, start))
                {
                    uniquePoints.Add(start);
                }

                if (!geometryService.ListContainsPoint(uniquePoints, start))
                {
                    uniquePoints.Add(end);
                }
            }

            var intersections = new List<StreetIntersection>();
            foreach (var point in uniquePoints)
            {
                var count = 0;
                var intersStreetSegments = new List<StreetSegment>();
                foreach (var segment in streetSegments)
                {
                    if (segment.Line.Distance(point) < 0.0001)
                    {
                        count++;
                        intersStreetSegments.Add(segment);
                    }
                }

                if (count > 1)
                {
                    var intersection = new StreetIntersection();
                    intersection.Id = osmStore.MoveNextId();
                    intersection.StreetIds = intersStreetSegments.Select(s => s.Id).Distinct().ToList();
                    intersection.NumberOfStreets = intersection.StreetIds.Count;
                    intersection.StreetSegmentIds = intersStreetSegments.Select(s => s.SegmentId).Distinct().ToList();
                    intersection.NumberOfStreetSegments = intersection.StreetSegmentIds.Count();
                    intersection.Origin = point;

                    var streetNames = intersStreetSegments.Select(s => s.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct();
                    var nameCount = streetNames.Count();
                    if (nameCount == 1)
                    {
                        intersection.Name = streetNames.ElementAt(0);
                    }
                    else if (nameCount >= 2)
                    {
                        intersection.Name = $"{streetNames.ElementAt(0)} with {streetNames.ElementAt(1)}";
                        for (int i = 2; i < nameCount; i++)
                        {
                            intersection.Name += $"and {streetNames.ElementAt(i)}";
                        }
                    }

                    intersection.CurveLoop = CreateIntersectionCurveLoop(intersStreetSegments, intersection);

                    intersections.Add(intersection);
                }
            }

            return intersections;
        }

        private CurveLoop CreateIntersectionCurveLoop(List<StreetSegment> intersStreetSegments, StreetIntersection intersection)
        {
            var arc = Arc.Create(intersection.Origin, osmStore.DefaultStreetWidth, 0, Math.PI * 1.99, XYZ.BasisX, XYZ.BasisY);

            var lineOffsetDic = new Dictionary<Line, double>();
            var offsetLines = new List<Line>();

            foreach (var segm in intersStreetSegments)
            {
                var line0 = segm.Line.CreateOffset(osmStore.DefaultStreetWidth / 2, XYZ.BasisZ) as Line;
                offsetLines.Add(line0);
                var line1 = segm.Line.CreateOffset(osmStore.DefaultStreetWidth / 2, -XYZ.BasisZ) as Line;
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

            var loop = new CurveLoop();
            for (int i = 0; i < orderedDic.Count - 1; i++)
            {
                var start = arc.Evaluate(orderedDic[i].Value, true);
                var end = arc.Evaluate(orderedDic[i + 1].Value, true);

                var line = Line.CreateBound(start, end);
                loop.Append(line);
            }

            return loop;
        }

        public XYZ ProjectLineToIntersection(Line line, StreetIntersection intersection)
        {
            if (intersection == null) return null;

            foreach (var curve in intersection.CurveLoop)
            {
                curve.Intersect(line, out IntersectionResultArray resultArray);
                if (resultArray != null && !resultArray.IsEmpty)
                {
                    var result = resultArray.get_Item(0);
                    return result.XYZPoint;
                }
            }

            return null;
        }
    }
}
