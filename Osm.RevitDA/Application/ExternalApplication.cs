using System;
using System.IO;

using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Osm.Revit.Store;
using Osm.Revit.Models;
using Osm.Revit.Services;

namespace Osm.Revit.Application
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class OsmApplication : IExternalDBApplication
    {
        private static string MapBoundsFile => "mapbounds.json";
        private static string ResultFile => "result.rvt";

        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            ContainerStore.Registration<MapStreamOnDemandService>();
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
                osmServie.Run(newDoc);
                t.Commit();
            }

            newDoc.SaveAs(ResultFile);
        }
    }
}