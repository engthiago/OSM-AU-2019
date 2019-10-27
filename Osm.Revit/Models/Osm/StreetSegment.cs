using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Models.Osm
{
    public class StreetSegment: OsmElement
    {
        public long SegmentId { get; set; }
        public double Width { get; set; }
        public Line Line { get; set; }
    }
}
