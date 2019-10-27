using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Store
{
    public static class OsmIdStore
    {
        public static int current;

        public static int MoveNext()
        {
            return ++current;
        }

        public static int GetLast()
        {
            return current;
        }
    }
}
