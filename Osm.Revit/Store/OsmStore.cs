using Autodesk.Revit.DB;
using Osm.Revit.Models;
using System;

namespace Osm.Revit.Store
{
    public class OsmStore
    {
        private static int currentId;

        public int LastId => currentId;
        public double Tau => 2 * Math.PI;
        public double RadiusEquator => 6356752.314245179;
        public double RadiusPolar => 6378137.0;

        public static MapBounds Bounds = null;

        public double MapTop => Bounds.Top;
        public double MapBottom => Bounds.Bottom;
        public double MapLeft => Bounds.Left;
        public double MapRight => Bounds.Right;

        public double DefaultStreetWidth { get; set; } = UnitUtils.ConvertToInternalUnits(6, DisplayUnitType.DUT_METERS);

        public double DefaultStreetThickness { get; set; } = UnitUtils.ConvertToInternalUnits(0.05, DisplayUnitType.DUT_METERS);

        public static void Geolocate(MapBounds mapBounds)
        {
            Bounds = mapBounds;
        }

        public int MoveNextId()
        {
            return ++currentId;
        }
    }
}
