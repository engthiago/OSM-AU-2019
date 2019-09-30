using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit
{
    static public class Lists
    {
        static public IList<Element> ToElements(this IList<ElementId> targetElementIdList, Document doc)
        {
            IList<Element> elementList = new List<Element>();
            foreach (ElementId currentElementId in targetElementIdList)
            {
                if (currentElementId == null)
                    continue;

                Element currentElement = doc.GetElement(currentElementId);
                elementList.Add(currentElement);
            }

            return elementList;
        }
    }
}
