using Autodesk.Revit.DB;
using Osm.Revit.Models;
using Osm.Revit.Models.Osm;
using Osm.Revit.Store;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class StreetService
    {
        private readonly GeometryService geometryService;
        private readonly CoordinatesService coordService;
        private readonly double defaultStreetWidth;

        public StreetService()
        {
            geometryService = new GeometryService();
            coordService = new CoordinatesService();
            defaultStreetWidth = UnitUtils.ConvertToInternalUnits(3000, DisplayUnitType.DUT_MILLIMETERS);
        }

        public List<StreetSegment> CreateStreetSegments(List<Way> OsmStreets, List<Node> nodes, CurveLoop boundLines, MapBounds mapBounds)
        {
            List<StreetSegment> streetSegments = new List<StreetSegment>();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            foreach (var osmStreet in OsmStreets)
            {
                var points = new List<XYZ>();
                foreach (var nodeId in osmStreet.Nodes)
                {
                    var geometry = nodes.FirstOrDefault(n => n.Id == nodeId);
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
                    segment.SegmentId = OsmIdStore.MoveNext();
                    segment.Line = line;
                    segment.Width = defaultStreetWidth;
                    if (osmStreet.Tags.TryGetValue("name", out string name))
                    {
                        segment.Name = name;
                    }

                    streetSegments.Add(segment);
                }
            }

            return streetSegments;
        }

        public List<StreetIntersection> GetIntersections(List<StreetSegment> streetSegments)
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
                    intersection.Id = OsmIdStore.MoveNext();
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

                    intersections.Add(intersection);
                }
            }



            return intersections;
        }
    }
}
