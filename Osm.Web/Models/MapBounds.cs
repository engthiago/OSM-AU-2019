using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Models
{
    public class MapBounds
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Right { get; set; }
    }
}
