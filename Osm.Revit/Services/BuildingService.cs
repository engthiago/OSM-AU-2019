using Autodesk.Revit.DB;
using OsmSharp;
using OsmSharp.Tags;
using System.Collections.Generic;
using System.Linq;

namespace Osm.Revit.Services
{
    public class BuildingService
    {
        private readonly CoordinatesService coordService;
        private readonly SolidGeometryService shapeService;

        public BuildingService(CoordinatesService coordService, SolidGeometryService shapeService)
        {
            this.coordService = coordService;
            this.shapeService = shapeService;
        }

        public List<DirectShape> Run(Document doc, List<OsmGeo> everything)
        {
            var solids = new List<DirectShape>();
            var buildings = everything.Where(n => n is Way && n.Tags.IsTrue("building")).Cast<Way>();
            var allNodes = everything.ToDictionary(g => g.Id, g => g);
            foreach (var building in buildings)
            {
                var solid = RunBuilding(doc, building, allNodes);
                if (solid == null)
                    continue;
                solids.Add(solid);
            }
            return solids;
        }

        private DirectShape RunBuilding(Document doc, Way building, Dictionary<long ?, OsmGeo> allNodes)
        {
            var points = new List<XYZ>();
            foreach (var nodeId in building.Nodes)
            {
                var geometry = allNodes[nodeId];
                if (geometry is Node node)
                {
                    var coords = coordService.GetRevitCoords((double)node.Latitude, (double)node.Longitude);
                    points.Add(coords);
                }
            }

            var curveLoop = new CurveLoop();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                curveLoop.Append(line);
            }

            var heightTag = building.Tags.FirstOrDefault(tag => tag.Key == "height");
            var heightFeet = UnitUtils.ConvertToInternalUnits(3, DisplayUnitType.DUT_METERS);
            if (double.TryParse(heightTag.Value, out double heightMeters))
            {
                heightFeet = UnitUtils.ConvertToInternalUnits(heightMeters, DisplayUnitType.DUT_METERS);
            }

            return shapeService.Build(doc, new List<CurveLoop> { curveLoop }, heightFeet, new ElementId(BuiltInCategory.OST_GenericModel));
        }

    }
}
