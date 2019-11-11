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
        private readonly string baseUrl;

        public DAController(DaService daService, MailService mailService, IHostingEnvironment env)
        {
            this.daService = daService;
            this.mailService = mailService;
            this.env = env;

            baseUrl = "https://osmdemo.azurewebsites.net";
            if (env.IsDevelopment())
            {
                baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            }
        }

        [HttpPut()]
        public async Task<IActionResult> Results(string email)
        {
            var dateStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var fileName = $"osm-{dateStamp}.rvt";
            using (var fs = new FileStream("wwwroot/downloads/" + fileName, FileMode.Create))
            {
                await Request.Body.CopyToAsync(fs);
            }

            var downloadUrl = this.baseUrl + "/downloads/" + fileName;

            await mailService.SendWorkCompleteEmail(email, downloadUrl);

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
