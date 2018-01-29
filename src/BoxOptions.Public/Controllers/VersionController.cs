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
                string coefStatus = "CoefficientCalculator is OK";
                try { var result = await _coefficientCalculator.RequestAsync("123456", "EURCHF"); }
                catch (Exception ex01){ coefStatus = $"Coefficient calculator status failed: {ex01.Message}"; }

                var coefCalc = _settings.CoefficientCalculator.Instruments.Select(x => x.Name).ToList();
                coefCalc.Insert(0, coefStatus);

                var answer = new VersionModel
                {
                    Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                    CoefficientCalculatorInstruments = coefCalc.ToArray(),
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
