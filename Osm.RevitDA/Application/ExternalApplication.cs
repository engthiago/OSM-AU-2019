using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Osm.Revit.Models;

namespace Osm.Revit.Application
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class OsnApplication : IExternalDBApplication
    {
        private static string OsmDetailsFile   => "osmdetails.xml";
        private static string MapBoundsFile => "mapbounds.json";

        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            GetOsmXml();
            e.Succeeded = true;
        }

        private static bool WaitForInput()
        {
            int idx = 0;
            while (true)
            {
                char ch = Convert.ToChar(Console.Read());
                if (ch == '\n')
                    return true; // found lf.
                else if (ch == '\x3')
                    return false; // error, cancelled.
                if (idx >= 16)
                    return false; // failed, after retries.
                idx++;
            }
        }

        private static bool FindFileInFolder(string filename)
        {
            return Directory.GetFiles(".").Any(f => f == $".\\{filename}");
        }

        private static void GetOsmXml()
        {
            MapBounds mapBounds = MapBounds.Deserialize(File.ReadAllText(MapBoundsFile));
            string suffix = $"map?bbox={mapBounds.Left}%2C{mapBounds.Bottom}%2C{mapBounds.Right}%2C{mapBounds.Top}";

            Console.WriteLine($"!ACESAPI:acesHttpOperation(osmParam,{suffix},,,file://{OsmDetailsFile})");

            if (!WaitForInput())
                throw new Exception($"Error in getting {OsmDetailsFile}");

            Console.WriteLine(File.ReadAllText(OsmDetailsFile));
        }
    }
}