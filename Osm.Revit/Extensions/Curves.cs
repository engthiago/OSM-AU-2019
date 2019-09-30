using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace Osm.Revit
{
    static public class Curves
    {
        static public CurveLoop CreateCurveLoop(this Curve firstCurve, Curve secondCurve)
        {
            CurveLoop curveLoop = new CurveLoop();

            curveLoop.Append(firstCurve.CreateReversed());
            curveLoop.Append(Line.CreateBound(firstCurve.GetEndPoint(0), secondCurve.GetEndPoint(0)));
            curveLoop.Append(secondCurve);
            curveLoop.Append(Line.CreateBound(secondCurve.GetEndPoint(1), firstCurve.GetEndPoint(1)));

            return curveLoop;
        }

        static public bool IsVertical(this Curve curve)
        {
            XYZ firstPoint = curve.GetEndPoint(0);
            firstPoint = new XYZ(firstPoint.X, firstPoint.Y, 0);
            XYZ secondPoint = curve.GetEndPoint(1);
            secondPoint = new XYZ(secondPoint.X, secondPoint.Y, 0);

            if (firstPoint.IsAlmostEqualTo(secondPoint))
                return true;

            return false;
        }

        static public Line Extend(this Line line, double extensionDistance)
        {
            XYZ firstPoint = line.GetEndPoint(0);
            XYZ secondPoint = line.GetEndPoint(1);
            XYZ direction = line.Direction;

            firstPoint = firstPoint.Add(-direction.Multiply(extensionDistance));
            secondPoint = secondPoint.Add(direction.Multiply(extensionDistance));

            return Line.CreateBound(firstPoint, secondPoint);
        }

        static public Curve Flatten(this Curve curve, double height = 0)
        {
            XYZ firstPoint = curve.GetEndPoint(0);
            firstPoint = new XYZ(firstPoint.X, firstPoint.Y, height);
            XYZ secondPoint = curve.GetEndPoint(1);
            secondPoint = new XYZ(secondPoint.X, secondPoint.Y, height);

            if (firstPoint.IsAlmostEqualTo(secondPoint))
                return null;

            if (curve is Line)
            {
                return Line.CreateBound(firstPoint, secondPoint);
            }
            if (curve is Arc)
            {
                XYZ centerPoint = curve.Evaluate(0.5, true);
                centerPoint = new XYZ(centerPoint.X, centerPoint.Y, height);
                return Arc.Create(firstPoint, secondPoint, centerPoint);
            }

            throw new Exception("THIAGO -> Curve can't be flatten because it is not a line or arc");
        }

        static public IList<Curve> Project(this Curve curve, Plane plane)
        {
            IList<Curve> pCurves = new List<Curve>();
            Curve pCurve = curve.Clone();
            XYZ pPoint0 = pCurve.GetEndPoint(0).Project(plane);
            XYZ pPoint1 = pCurve.GetEndPoint(1).Project(plane);

            XYZ pPointCenter = pCurve.Evaluate(0.5, true).Project(plane);

            //If the the curve is a line , just return the line
            //If the distance between the center point is less or equal 3 cm, there is no point in creating more complex geometry
            if (curve as Line != null || pPoint0.DistanceTo(pPoint1) <= 0.1)
            {
                pCurves.Add(Line.CreateBound(pPoint0, pPoint1));
                return pCurves;
            }

            Line line0 = Line.CreateBound(pPoint0, pPointCenter);
            Line line1 = Line.CreateBound(pPointCenter, pPoint1);

            //If the curve was not a line but became one after the projection, we just return a line
            if (Math.Abs(line0.Direction.DotProduct(line1.Direction)).IsAlmostEqual(1))
            {
                pCurves.Add(Line.CreateBound(pPoint0, pPoint1));
            }
            else if (curve as Arc != null)
            {
                Arc arc = Arc.Create(pPoint0, pPoint1, pPointCenter);
            }
            else
            {
                pCurves.Add(line0);
                pCurves.Add(line1);
            }

            return pCurves;
        }

    }
}
