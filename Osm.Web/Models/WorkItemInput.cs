using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Models
{
    public class WorkItemInput
    {
        public string Email { get; set; }
        public MapBounds MapBounds { get; set; }
    }
}
