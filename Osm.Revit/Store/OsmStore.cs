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

        private static MapBounds mapBounds = null;

        public double MapTop => mapBounds.Top;
        public double MapBottom => mapBounds.Bottom;
        public double MapLeft => mapBounds.Left;
        public double MapRight => mapBounds.Right;

        public double DefaultStreetWidth { get; set; } = UnitUtils.ConvertToInternalUnits(6, DisplayUnitType.DUT_METERS);

        public double DefaultStreetThickness { get; set; } = UnitUtils.ConvertToInternalUnits(0.05, DisplayUnitType.DUT_METERS);

        public static void Geolocate(MapBounds mapBounds)
        {
            OsmStore.mapBounds = mapBounds;
        }

        public int MoveNextId()
        {
            return ++currentId;
        }
    }
}
