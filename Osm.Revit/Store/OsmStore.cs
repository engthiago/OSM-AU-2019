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

        private static MapBounds mapBounds = new MapBounds
        {
            Left = -73.85282,
            Bottom = 40.68363,
            Right = -73.84965,
            Top = 40.68585,
        };

        public double MapTop => mapBounds.Top;
        public double MapBottom => mapBounds.Bottom;
        public double MapLeft => mapBounds.Left;
        public double MapRight => mapBounds.Right;




        public void Geolocate(MapBounds mapBounds)
        {
            OsmStore.mapBounds = mapBounds;
        }

        public int MoveNextId()
        {
            return ++currentId;
        }
    }
}
