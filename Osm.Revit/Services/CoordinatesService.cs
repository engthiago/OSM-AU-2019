using Autodesk.Revit.DB;
using Osm.Revit.Store;
using System;

namespace Osm.Revit.Services
{
    public class CoordinatesService
    {
        private readonly OsmStore store;

        public CoordinatesService(OsmStore store)
        {
            this.store = store;
        }

        private double GeoLatToMeters(double dLat)
        {
            return dLat * (store.Tau * store.RadiusPolar / 360);
        }

        private double GeoLongToMeteres(double dLon, double atLat)
        {
            return Math.Abs(atLat) >= 90 ? 0 :
                    dLon * (store.Tau * store.RadiusEquator / 360) * Math.Abs(Math.Cos(atLat * (Math.PI / 180)));
        }

        public XYZ GetRevitCoords(double lati, double longi)
        {
            var dlong = GeoLongToMeteres(longi - store.MapLeft, store.MapBottom);
            var dlat = GeoLatToMeters(lati - store.MapBottom);

            var x = UnitUtils.ConvertToInternalUnits(dlong, DisplayUnitType.DUT_METERS);
            var y = UnitUtils.ConvertToInternalUnits(dlat, DisplayUnitType.DUT_METERS);

            return new XYZ(x, y, 0);
        }
    }
}
