using System;
using System.Threading.Tasks;
using BoxOptions.Common;
using BoxOptions.Public.Models;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class CoefController : Controller
    {
        private readonly BoxOptionsSettings _settings;
        Processors.ICoefficientCalculator coefCalculator;

        public CoefController(BoxOptionsSettings settings)
        {
            _settings = settings;
            if (_settings.BoxOptionsApi.CoefApiUrl.ToLower() == "mock")
                coefCalculator = new Processors.MockCoefficientCalculator();            
            else
                coefCalculator = new Processors.ProxyCoefficientCalculator(_settings);
        }
      
        [HttpGet]
        [Route("change")]
        public async Task<IActionResult> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex, string userId="0")
        {
            try
            {
                var result = await coefCalculator.ChangeAsync(pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex, userId);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
        
        [HttpGet]
        [Route("request")]
        public async Task<IActionResult> RequestAsync(string pair, string userId = "0")
        {
            try
            {
                var result = await coefCalculator.RequestAsync(pair, userId);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
