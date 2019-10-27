using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Models.Osm
{
    public class StreetIntersection: OsmElement
    {
        public XYZ Origin { get; set; }

        public int NumberOfStreetSegments { get; set; }
        public int NumberOfStreets { get; set; }

        public List<long> StreetSegmentIds { get; set; }
        public List<long> StreetIds { get; set; }
    }
}
