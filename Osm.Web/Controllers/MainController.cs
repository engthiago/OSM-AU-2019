using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Osm.Web.Controllers
{
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly IHostingEnvironment env;

        public MainController(IHostingEnvironment env)
        {
            this.env = env;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var root = env.WebRootPath;
            var path = System.IO.Path.Combine(root, "index.hmtl");

            return File(path, "text/hml");
        }
    }
}
