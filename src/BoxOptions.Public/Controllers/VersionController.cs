using BoxOptions.Public.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoxOptions.Public.Controllers
{
    
    public class VersionController : Controller
    {
        [HttpGet]
        [Route("home/version")]
        [Route("api/IsAlive")]
        public VersionModel Get()
        {
            return new VersionModel
            {
                Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
            };
        }
    }
}
