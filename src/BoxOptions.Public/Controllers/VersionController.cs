using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Public.Models;
using BoxOptions.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{

    public class VersionController : Controller
    {
        private readonly BoxOptionsApiSettings _settings;
        ICoefficientCalculator _coefficientCalculator;

        public VersionController(BoxOptionsApiSettings settings, ICoefficientCalculator coefficientCalculator)
        {            
            _settings = settings;
            _coefficientCalculator = coefficientCalculator;
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

                var result = await _coefficientCalculator.RequestAsync("123456", "EURCHF");
                
                var answer = new VersionModel
                {
                    Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                    CoefficientCalculatorInstruments = _settings.CoefficientCalculator.Instruments.Select(x => x.Name).ToArray(),
                    DaysHistory = _settings.HistoryHolder.NumberOfDaysInCache
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
