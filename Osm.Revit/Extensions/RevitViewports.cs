using Autodesk.Revit.DB;

namespace Osm.Revit
{
    public static class RevitViewports
    {
        public static double GetWidth(this Viewport viewport)
        {
            if (viewport == null) return 0;
            Outline boundingBoxXYZ = viewport.GetBoxOutline();

            XYZ min = boundingBoxXYZ.MinimumPoint.Flatten();
            XYZ max = boundingBoxXYZ.MaximumPoint.Flatten();

            XYZ side = new XYZ(max.X, min.Y, 0);

            double distance = min.DistanceTo(side);
            return distance;
        }

        public static double GetHeight(this Viewport viewport)
        {
            if (viewport == null) return 0;
            Outline boundingBoxXYZ = viewport.GetBoxOutline();

            XYZ min = boundingBoxXYZ.MinimumPoint.Flatten();
            XYZ max = boundingBoxXYZ.MaximumPoint.Flatten();

            XYZ side = new XYZ(max.X, min.Y, 0);

            double distance = max.DistanceTo(side);
            return distance;
        }
    }
}
