using Newtonsoft.Json;
using Osm.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Web.Services
{
    public class ModelDerivativeService
    {
        static private HttpClient httpClient;
        private readonly ForgeAuth forgeAuth;

        public ModelDerivativeService(ForgeAuth forgeAuth)
        {
            httpClient = new HttpClient();
            this.forgeAuth = forgeAuth;
        }

        public async Task TranslateForTheViewer(string urn)
        {
            var authentication = await this.forgeAuth.GetFullAuthorization();
            var url = $"https://developer.api.autodesk.com/modelderivative/v2/designdata/job";

            var payload = new ModelDerivativePayload
            {
                Input = new Input { Urn = urn },
                Output = new Output
                {
                    Formats = new List<Format>()
                    {
                        new Format { Type = "svf", Views = new List<string> { "2d", "3d" } }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authentication.AccessToken}");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return;
            }
            else
            {
                throw new Exception("Could not translate file \n" + await response.Content.ReadAsStringAsync());
            }
        }
    }
}
