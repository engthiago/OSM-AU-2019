using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Osm.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Osm.Web.Services
{
    public class DaService
    {
        private readonly DesignAutomationClient daClient;

        public DaService(DesignAutomationClient daClient)
        {
            this.daClient = daClient;
        }

        public async Task<WorkItemStatus> SendWorkItem(MapBounds bounds, string email, string baseUrl)
        {
            var mapParam = new XrefTreeArgument
            {
                Url = $"data:application/json,{{'left':{bounds.Left},'top':{bounds.Top},'right':{bounds.Right},'bottom':{bounds.Bottom}}}",
                Verb = Verb.Get
            };

            var osmParam = new XrefTreeArgument
            {
                Url = "https://api.openstreetmap.org/api/0.6",
                Verb = Verb.Get
            };

            var result = new XrefTreeArgument
            {
                Url = $"{baseUrl}/api/da?email={email}",
                Verb = Verb.Put
            };

            var workItem = new WorkItem
            {
                ActivityId = "OsmDemo.OsmActivity+test",
                Arguments = new Dictionary<string, IArgument>()
                {
                    { "mapParam", mapParam },
                    { "osmParam", osmParam },
                    { "result", result }
                }
            };

            var status = await this.daClient.CreateWorkItemAsync(workItem);
            return status;
        }
    }
}
