using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit
{
    static public class Points
    {
        static public XYZ Flatten(this XYZ point, double height = 0)
        {
            return new XYZ(point.X, point.Y, height);
        }

        static public XYZ Project(this XYZ point, Plane plane)
        {
            XYZ v = plane.Origin - point;
            XYZ normal = plane.Normal;
            double magnitude = Math.Abs(normal.GetLength());

            XYZ vp = normal.Multiply(v.DotProduct(normal) / (magnitude * magnitude));
            XYZ projection = point + vp;

            return projection;
        }

        static public XYZ Project(this XYZ vector, XYZ vectorV)
        {
            double magnitude = Math.Abs(vectorV.GetLength());
            XYZ projection = vectorV.Multiply(vector.DotProduct(vectorV) / (magnitude * magnitude));

            return projection;
        }
    }
}
