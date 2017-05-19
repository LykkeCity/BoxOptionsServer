using BoxOptions.Common;
using BoxOptions.Public.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{

    public class VersionController : Controller
    {
        private readonly BoxOptionsSettings _settings;
        Processors.ICoefficientCalculator coefCalculator;

        public VersionController(BoxOptionsSettings settings)
        {
            _settings = settings;
            if (_settings.BoxOptionsApi.CoefApiUrl.ToLower() == "mock")
                coefCalculator = new Processors.MockCoefficientCalculator();
            else
                coefCalculator = new Processors.ProxyCoefficientCalculator(_settings);
        }

        [HttpGet]        
        [Route("home/version")]
        [Route("api/IsAlive")]
        public async Task<IActionResult> Get()
        {
            try
            {                
                // CoefAPI test for slack logging
                // TODO: remove API test
                var result = await coefCalculator.RequestAsync("123456", "EURUSD");

                var answer = new VersionModel
                {
                    Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
                };
                return  Ok(answer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            };
        }
    }
}
