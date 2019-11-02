using Autodesk.Revit.DB;
using Osm.Revit.Models;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit.Services
{
    public class GeometryService
    {
        readonly CoordinatesService coordService;

        public GeometryService(CoordinatesService coordService)
        {
            this.coordService = coordService;
        }

        public CurveLoop CreateBoundingLines(MapBounds mapBounds)
        {
            var bb0 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Left);
            var bb1 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Left);
            var bb2 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Right);
            var bb3 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Right);

            var l0 = Line.CreateBound(bb0, bb1);
            var l1 = Line.CreateBound(bb1, bb2);
            var l2 = Line.CreateBound(bb2, bb3);
            var l3 = Line.CreateBound(bb3, bb0);

            var loop = new CurveLoop();
            loop.Append(l0);
            loop.Append(l1);
            loop.Append(l2);
            loop.Append(l3);

            return loop;
        }

        /// <summary>
        /// Verify if a given point is inside the bounding poylgon
        /// </summary>
        /// <remarks>
        /// https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/#targetText=1)%20Draw%20a%20horizontal%20line,true%2C%20then%20point%20lies%20outside.
        /// </remarks>
        /// <param name="boundingPolygon">Bounding Polygon Curves</param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInsideBounds(CurveLoop boundingPolygon, XYZ point)
        {
            var intersections = 0;
            var lineDir = XYZ.BasisX;

            var start = point;
            var end = point.Add(lineDir.Multiply(100000));
            Line line = Line.CreateBound(start, end);

            foreach (var curve in boundingPolygon)
            {
                // Check if the curve is on the edge of the line
                if (curve.Distance(point) < 0.00001) return true;

                curve.Intersect(line, out IntersectionResultArray intersectionArr);
                if (intersectionArr != null)
                {
                    intersections += intersectionArr.Size;
                }
            }

            // Checks if the number of intersections is odd
            if (intersections % 2 != 0) return true;

            return false;
        }

        public bool ListContainsPoint(List<XYZ> list, XYZ point)
        {
            return list.FirstOrDefault(p => p.IsAlmostEqualTo(point)) != null;
        }

    }
}
