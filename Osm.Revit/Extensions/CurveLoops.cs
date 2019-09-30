using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit
{
    static class CurveLoops
    {
        static public Line FlattenIntoLineBound(this CurveLoop curveLoop, double height = 0)
        {
            IList<Curve> flattenCurves = curveLoop.Select(c => c.Flatten(height)).Where(c => c != null).ToList();
            double maxDist = double.MinValue;
            Line flattenLine = null;

            foreach (Curve curve1 in flattenCurves)
            {
                foreach (Curve curve2 in flattenCurves)
                {
                    XYZ c1P0 = curve1.GetEndPoint(0);
                    XYZ c1P1 = curve1.GetEndPoint(1);

                    XYZ c2P0 = curve2.GetEndPoint(0);
                    XYZ c2P1 = curve2.GetEndPoint(1);

                    double dist = c1P0.DistanceTo(c1P1);
                    if (dist > maxDist && dist > 0.05)
                    {
                        maxDist = dist;
                        flattenLine = Line.CreateBound(c1P0, c1P1);
                    }

                    dist = c1P0.DistanceTo(c2P0);
                    if (dist > maxDist && dist > 0.05)
                    {
                        maxDist = dist;
                        flattenLine = Line.CreateBound(c1P0, c2P0);
                    }

                    dist = c1P0.DistanceTo(c2P1);
                    if (dist > maxDist && dist > 0.05)
                    {
                        maxDist = dist;
                        flattenLine = Line.CreateBound(c1P0, c2P1);
                    }
                }
            }

            return flattenLine;
        }

        static public IList<XYZ> FlattenIntoPoints(this CurveLoop curveLoop, double height = 0, double tolerance = 0.0328)
        {
            IList<XYZ> points = new List<XYZ>();

            foreach (Curve curve in curveLoop)
            {
                XYZ point0 = curve.GetEndPoint(0);
                XYZ point1 = curve.GetEndPoint(1);

                point0 = point0.Flatten(height);
                point1 = point1.Flatten(height);

                if (points.Where(p => p.IsAlmostEqualTo(point0, tolerance)).FirstOrDefault() == null)
                {
                    points.Add(point0);
                }

                if (points.Where(p => p.IsAlmostEqualTo(point1, tolerance)).FirstOrDefault() == null)
                {
                    points.Add(point1);
                }

            }

            return points;
        }

        static public IList<XYZ> FlattenIntoPoints(this IList<CurveLoop> curveLoops, double height = 0, double tolerance = 0.0328)
        {
            IList<XYZ> points = new List<XYZ>();

            foreach (CurveLoop curveLoop in curveLoops)
            {
                IList<XYZ> curveloopPoints = curveLoop.FlattenIntoPoints(height, tolerance);
                foreach (XYZ curveloopPoint in curveloopPoints)
                {
                    if (points.Where(p => p.IsAlmostEqualTo(curveloopPoint, tolerance)).FirstOrDefault() == null)
                    {
                        points.Add(curveloopPoint);
                    }
                }
            }

            return points;
        }

        static public IList<Curve> ProjectAsCurveList(this CurveLoop curveLoop, Plane plane)
        {
            IList<Curve> pCurveLoop = new List<Curve>();

            foreach (Curve curve in curveLoop)
            {
                IList<Curve> pCurve = curve.Clone().Project(plane);

                pCurveLoop = pCurveLoop.Union(pCurve).ToList();

            }

            return pCurveLoop;
        }

        static public CurveLoop ProjectAsCurveLoop(this CurveLoop curveLoop, Plane plane)
        {
            CurveLoop pCurveLoop = new CurveLoop();
            foreach (Curve curve in curveLoop)
            {
                IList<Curve> pCurves = curve.Clone().Project(plane);
                foreach (Curve pCurve in pCurves)
                {
                    pCurveLoop.Append(pCurve);
                }
            }

            return pCurveLoop;
        }

        static public IList<CurveLoop> ProjectCurveLoops(this IList<CurveLoop> curveLoops, Plane plane)
        {
            IList<CurveLoop> pCurveLoops = new List<CurveLoop>();
            foreach (CurveLoop curveloop in curveLoops)
            {
                CurveLoop pCurveLoop = curveloop.ProjectAsCurveLoop(plane);
                pCurveLoops.Add(pCurveLoop);
            }

            return pCurveLoops;
        }

    }
}
