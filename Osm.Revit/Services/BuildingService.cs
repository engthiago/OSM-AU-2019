﻿using Autodesk.Revit.DB;
using Osm.Revit.Store;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach (var building in buildings)
            {
                var solid = RunBuilding(doc, building, everything);
                solids.Add(solid);
            }
            return solids;
        }

        private DirectShape RunBuilding(Document doc, Way building, List<OsmGeo> everything)
        {
            var points = new List<XYZ>();
            foreach (var nodeId in building.Nodes)
            {
                var geometry = everything.FirstOrDefault(n => n.Id == nodeId);
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