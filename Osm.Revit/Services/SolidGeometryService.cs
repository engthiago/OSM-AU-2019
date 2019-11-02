using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class SolidGeometryService
    {
        public SolidGeometryService()
        {
        }

        public DirectShape Build(Document doc, List<CurveLoop> profile, double height, ElementId catId)
        {
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(profile, XYZ.BasisZ, height);
            var directShape = DirectShape.CreateElement(doc, catId);
            directShape.AppendShape(new List<GeometryObject> { solid });
            return directShape;
        }
    }
}
