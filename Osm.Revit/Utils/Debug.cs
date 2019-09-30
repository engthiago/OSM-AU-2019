using Autodesk.Revit.DB;
using System;
using System.Linq;

namespace Osm.Revit.Utils
{
    public class Debug
    {
        public static void CreateDebugPoint(Document doc, XYZ point)
        {
            if (doc == null) throw new Exception("DEBUG POINT -> NULL DOC");
            if (point == null) throw new Exception("DEBUG POINT -> NULL POINT");

            FamilySymbol fs = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                    .Where(f => f.Name.ToLower().Contains("debug")).FirstOrDefault() as FamilySymbol;

            if (fs == null) throw new Exception("DEBUG POINT -> NO DEBUG POINT FOUND ON DOCUMENT");

            fs.Activate();

            doc.Create.NewFamilyInstance(point, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }
    }
}
