using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TopoCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var streetWidth = UnitUtils.ConvertToInternalUnits(6000, DisplayUnitType.DUT_MILLIMETERS);

            var httpService = new HttpService();
            var mapBounds = new MapBounds
            {
                Left = -73.85282,
                Bottom = 40.68363,
                Right = -73.84965,
                Top = 40.68585,
            };

            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);
            var bb0 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Left);
            var bb1 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Left);
            var bb2 = coordService.GetRevitCoords(mapBounds.Top, mapBounds.Right);
            var bb3 = coordService.GetRevitCoords(mapBounds.Bottom, mapBounds.Right);

            var points = new List<XYZ>()
            {
                bb0, bb1, bb2, bb3
            };

            using (Transaction t = new Transaction(doc, "Create Base Topo"))
            {
                t.Start();
                TopographySurface.Create(doc, points);
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
