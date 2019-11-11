using Osm.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Web.Services
{
    public class DataManagementService
    {
        static private HttpClient httpClient;
        private readonly ForgeAuth forgeAuth;

        public DataManagementService(ForgeAuth forgeAuth)
        {
            httpClient = new HttpClient();
            this.forgeAuth = forgeAuth;
        }

        public async Task<UploadFileBucketResult> UploadFileToBucket(string fileName, string bucket, Stream content)
        {
            var authentication = await this.forgeAuth.GetFullAuthorization();
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{bucket}/objects/{fileName}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authentication.AccessToken}");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await httpClient.PutAsync(url, new StreamContent(content));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<UploadFileBucketResult>();
                return result;
            }
            else
            {
                throw new Exception("Could not upload file to bucket");
            }
        }

        public async Task<DataManagementSignedUrl> CreateSignedUrl(string bucket, string objectkey)
        {
            var authentication = await this.forgeAuth.GetFullAuthorization();
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{bucket}/objects/{objectkey}/signed";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authentication.AccessToken}");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await httpClient.PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<DataManagementSignedUrl>();
                return result;
            }
            else
            {
                throw new Exception("Failed to create signed URL");
            }
        }
    }
}
