using Autodesk.Revit.DB;
using OsmSharp;
using Osm.Revit.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit.Services
{
    public class GeometryService
    {
        readonly CoordinatesService coordService;
        private readonly OsmStore osmStore;

        public GeometryService(CoordinatesService coordService, OsmStore osmStore)
        {
            this.coordService = coordService;
            this.osmStore = osmStore;
        }

        public CurveLoop CreateBoundingLines()
        {
            var bb0 = coordService.GetRevitCoords(osmStore.MapBottom, osmStore.MapLeft);
            var bb1 = coordService.GetRevitCoords(osmStore.MapTop, osmStore.MapLeft);
            var bb2 = coordService.GetRevitCoords(osmStore.MapTop, osmStore.MapRight);
            var bb3 = coordService.GetRevitCoords(osmStore.MapBottom, osmStore.MapRight);

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


        [Flags]
        private enum OutCode
        {
            Inside = 0,
            Left = 1,
            Right = 2,
            Bottom = 4,
            Top = 8
        }

        private OutCode ComputeOutCode(UV coords)
        {
            OutCode code = OutCode.Inside;

            if (coords.U < osmStore.MapLeft)
                code |= OutCode.Left;
            else if (coords.U > osmStore.MapRight)
                code |= OutCode.Right;
            if (coords.V < osmStore.MapBottom)
                code |= OutCode.Bottom;
            else if (coords.V > osmStore.MapTop)
                code |= OutCode.Top;

            return code;
        }

        List<UV> CohenSutherlandClipSegments(UV c0, UV c1)
        {
            OutCode outcode0 = ComputeOutCode(c0);
            OutCode outcode1 = ComputeOutCode(c1);
            bool accept = false;
            bool second = false;

            while (true)
            {
                if ((outcode0 | outcode1) == OutCode.Inside)
                {
                    accept = true;
                    break;
                }
                else if ((outcode0 & outcode1) != OutCode.Inside)
                {
                    break;
                }
                else
                {
                    double x, y;

                    OutCode outcodeOut = outcode0 != OutCode.Inside ? outcode0 : outcode1;

                    if ((outcodeOut & OutCode.Top) != OutCode.Inside)
                    {
                        x = c0.U + (c1.U - c0.U) * (osmStore.MapTop - c0.V) / (c1.V - c0.V);
                        y = osmStore.MapTop;
                    }
                    else if ((outcodeOut & OutCode.Bottom) != OutCode.Inside)
                    {
                        x = c0.U + (c1.U - c0.U) * (osmStore.MapBottom - c0.V) / (c1.V - c0.V);
                        y = osmStore.MapBottom;
                    }
                    else if ((outcodeOut & OutCode.Right) != OutCode.Inside)
                    {
                        y = c0.V + (c1.V - c0.V) * (osmStore.MapRight - c0.U) / (c1.U - c0.U);
                        x = osmStore.MapRight;
                    }
                    else /* if ((outcodeOut & OutCode.Left) != OutCode.Inside) */
                    {
                        y = c0.V + (c1.V - c0.V) * (osmStore.MapLeft - c0.U) / (c1.U - c0.U);
                        x = osmStore.MapLeft;
                    }

                    if (outcodeOut == outcode0)
                    {
                        c0 = new UV(x, y);
                        outcode0 = ComputeOutCode(c0);
                    }
                    else
                    {
                        c1 = new UV(x, y);
                        second = true;
                        outcode1 = ComputeOutCode(c1);
                    }
                }
            }

            List<UV> coords = new List<UV>();
            if (accept)
            {
                coords.Add(c0);
                if (second)
                    coords.Add(c1);
            }

            return coords;
        }

        public List<UV> GetCoordsFromNodeIds(long[] nodeIds, Dictionary<long?, OsmGeo> allNodes) => nodeIds.Where(id => allNodes[id] is Node).Select(id => allNodes[id] as Node).Select(n => new UV(n.Longitude.Value, n.Latitude.Value)).ToList();

        public List<XYZ> GetPointsFromNodes(long[] nodeIds, Dictionary<long?, OsmGeo> allNodes)
        {
            var coords = GetCoordsFromNodeIds(nodeIds, allNodes);
            var points = new List<XYZ>();
            for (int i=0; i<coords.Count-1; i++)
            {
                points.AddRange(CohenSutherlandClipSegments(coords[i], coords[i+1]).Select(c => coordService.GetRevitCoords(c.V, c.U)));
            }
            if (points.Count > 1)
                points.Add(points[0]);
            return points;
        }

    }
}
