using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Osm.Web.Models;
using Osm.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class DAController : ControllerBase
    {
        private readonly DaService daService;
        private readonly MailService mailService;
        private readonly IHostingEnvironment env;
        private readonly DataManagementService dataManagementService;
        private readonly ModelDerivativeService modelDerivativeService;
        private readonly string baseUrl;

        public DAController(DaService daService, MailService mailService, IHostingEnvironment env, DataManagementService dataManagementService, ModelDerivativeService modelDerivativeService)
        {
            this.daService = daService;
            this.mailService = mailService;
            this.env = env;
            this.dataManagementService = dataManagementService;
            this.modelDerivativeService = modelDerivativeService;
            baseUrl = "https://osmdemo.azurewebsites.net";
            if (env.IsDevelopment())
            {
                baseUrl = "https://localhost:44319/api";
            }
        }

        [HttpPut()]
        public async Task<IActionResult> Results(string email)
        {
            var dateStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var fileName = $"osm-{dateStamp}.rvt";
            //using (var fs = new FileStream("wwwroot/downloads/" + fileName, FileMode.Create))
            //{
            //    await Request.Body.CopyToAsync(fs);
            //}

            var bucket = "osmdemo";

            var fileResult = await this.dataManagementService.UploadFileToBucket(fileName, bucket, this.Request.Body);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fileResult.ObjectId);
            var urn = Convert.ToBase64String(plainTextBytes).Replace("=", "").Replace("/", "_");

            await this.modelDerivativeService.TranslateForTheViewer(urn);

            var signedUrl = await this.dataManagementService.CreateSignedUrl(bucket, fileResult.ObjectKey);

            var downloadUrl = signedUrl.SignedUrl;
            var viewUrl = this.baseUrl + "/viewer.html?urn=" + urn;

            await mailService.SendWorkCompleteEmail(email, downloadUrl, viewUrl);

            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> PostWorkItem(WorkItemInput input)
        {
            var email = input.Email;
            var bounds = input.MapBounds;
            
            var status = await daService.SendWorkItem(bounds, email, this.baseUrl);

            return Ok(status);
        }
    }
}
