using System;
using System.IO;

using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Osm.Revit.Store;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp.Streams;

namespace Osm.Revit.Application
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class OsmApplication : IExternalDBApplication
    {
        private static string OsmDetailsFile   => "osmdetails.xml";
        private static string MapBoundsFile => "mapbounds.json";
        private static string ResultFile => "result.rvt";

        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            ContainerStore.Registration();
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            GenerateRevitFile(e.DesignAutomationData);
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

        private static void GenerateRevitFile(DesignAutomationData daData)
        {
            var rvtApp = daData.RevitApp;
            Document newDoc = rvtApp.NewProjectDocument(UnitSystem.Imperial) ?? throw new InvalidOperationException("Could not create new document.");

            var osmServie = ContainerStore.Resolve<OsmService>();

            using (Transaction t = new Transaction(newDoc, "Build City"))
            {
                t.Start();

                MapBounds mapBounds = MapBounds.Deserialize(File.ReadAllText(MapBoundsFile));
                OsmStore.Geolocate(mapBounds);
                osmServie.RunWithStream(newDoc, GetOsmXmlStream(mapBounds));
                t.Commit();
            }

            newDoc.SaveAs(ResultFile);
        }


        private static XmlOsmStreamSource GetOsmXmlStream(MapBounds mapBounds)
        {
            string suffix = $"map?bbox={mapBounds.Left}%2C{mapBounds.Bottom}%2C{mapBounds.Right}%2C{mapBounds.Top}";

            Console.WriteLine($"!ACESAPI:acesHttpOperation(osmParam,{suffix},,,file://{OsmDetailsFile})");

            if (!WaitForInput())
                throw new Exception($"Error in getting {OsmDetailsFile}");

            Console.WriteLine(File.ReadAllText(OsmDetailsFile));

            return new XmlOsmStreamSource(File.Open(OsmDetailsFile, FileMode.Open));
        }
    }
}