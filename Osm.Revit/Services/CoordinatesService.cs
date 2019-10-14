﻿using Autodesk.Revit.DB;
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
        readonly double tau = 2 * Math.PI;
        readonly double radiusEquator = 6356752.314245179;
        readonly double radiusPolar = 6378137.0;

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

        private double GeoLatToMeters(double dLat)
        {
            return dLat * (tau * radiusPolar / 360);
        }

        private double GeoLongToMeteres(double dLon, double atLat)
        {
            return Math.Abs(atLat) >= 90 ? 0 :
                    dLon * (tau * radiusEquator / 360) * Math.Abs(Math.Cos(atLat * (Math.PI / 180)));
        }

        public XYZ GetRevitCoords(double lati, double longi)
        {
            var dlong = GeoLongToMeteres(longi - this.bottom, this.left);
            var dlat = GeoLatToMeters(lati - this.left);

            var x = UnitUtils.ConvertToInternalUnits(dlong, DisplayUnitType.DUT_METERS);
            var y = UnitUtils.ConvertToInternalUnits(dlat, DisplayUnitType.DUT_METERS);

            return new XYZ(x, y, 0);
        }
    }
}