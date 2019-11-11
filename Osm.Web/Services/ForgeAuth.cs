using Microsoft.Extensions.Configuration;
using Osm.Web.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Osm.Web.Services
{
    public class ForgeAuth
    {
        static private ForgeToken viewerToken;
        static private ForgeToken fullToken;
        private readonly IConfiguration config;

        public ForgeAuth(IConfiguration config)
        {
            this.config = config;
        }

        private bool IsViewerAuthorized()
        {
            return !string.IsNullOrWhiteSpace(viewerToken?.AccessToken) && viewerToken?.Expiration > DateTime.UtcNow.AddSeconds(30);
        }

        private bool IsFullAuthorized()
        {
            return !string.IsNullOrWhiteSpace(fullToken?.AccessToken) && fullToken?.Expiration > DateTime.UtcNow.AddSeconds(30);
        }

        public async Task<ForgeToken> GetViewerAuthorization()
        {
            if (IsViewerAuthorized())
            {
                return viewerToken;
            }
            else
            {
                viewerToken = await Authorize("data:read bucket:read");
                return viewerToken;
            }
        }

        public async Task<ForgeToken> GetFullAuthorization()
        {
            if (IsFullAuthorized())
            {
                return fullToken;
            }
            else
            {
                fullToken = await Authorize("data:search data:read bucket:read bucket:create data:write bucket:delete account:read account:write");
                return fullToken;
            }
        }

        private async Task<ForgeToken> Authorize(string scope)
        {
            var encodedScope = WebUtility.UrlEncode(scope);
            var client = new RestClient("https://developer.api.autodesk.com/authentication/v1/authenticate");
            var request = new RestRequest(Method.POST);
            request.Timeout = 30000;
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined",
                $"client_id={this.config["Forge:ClientId"]}" +
                $"&client_secret={this.config["Forge:ClientSecret"]}" +
                $"&grant_type=client_credentials" +
                $"&scope={encodedScope}",
                ParameterType.RequestBody);
            IRestResponse<ForgeToken> response = await client.ExecuteTaskAsync<ForgeToken>(request);
            ForgeToken authentication = response.Data;
            authentication.Expiration = DateTime.UtcNow.AddSeconds(authentication.ExpiresIn);
            return response.Data;
        }

    }
}
