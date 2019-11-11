using Microsoft.AspNetCore.Mvc;
using Osm.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly ForgeAuth forgeAuth;

        public OauthController(ForgeAuth forgeAuth)
        {
            this.forgeAuth = forgeAuth;
        }

        [HttpGet("viewer")]
        public async Task<IActionResult> GetForgeToken()
        {
            var token = await forgeAuth.GetViewerAuthorization();
            return Ok(token);
        }


    }
}
