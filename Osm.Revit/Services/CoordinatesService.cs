using Autodesk.Revit.DB;
using Osm.Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class CoordinatesService
    {
        double left;
        double bottom;

        public CoordinatesService()
        {
        }

        public void Geolocate(double left, double bottom)
        {
            this.left = left;
            this.bottom = bottom;
        }

        public XYZ GetRevitCoords(double lati, double longi)
        {
            var y = UnitUtils.Convert((lati - this.left) * 100000, DisplayUnitType.DUT_METERS, DisplayUnitType.DUT_DECIMAL_FEET);
            var x = UnitUtils.Convert((longi - this.bottom) * 100000, DisplayUnitType.DUT_METERS, DisplayUnitType.DUT_DECIMAL_FEET);

            return new XYZ(x, y, 0);
        }
    }
}
