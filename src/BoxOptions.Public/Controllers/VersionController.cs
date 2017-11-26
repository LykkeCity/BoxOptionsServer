using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Public.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{

    public class VersionController : Controller
    {
        private readonly BoxOptionsApiSettings _settings;
        ICoefficientCalculator coefCalculator;

        public VersionController(BoxOptionsApiSettings settings)
        {
            _settings = settings;
            if (_settings.CoefApiUrl.ToLower() == "mock")
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

                var result = await coefCalculator.RequestAsync("123456", "EURCHF");


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
