using System.Collections.Generic;

using Osm.Revit.Models;
using OsmSharp;


namespace Osm.Revit.Services
{
    public interface IMapStreamService
    {
        List<OsmGeo> GetOsmGeoList(MapBounds mapbounds);
    }
}
