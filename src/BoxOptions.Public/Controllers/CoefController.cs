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
#if DEBUG
            //coefCalculator = new Processors.MockCoefficientCalculator();
            coefCalculator = new Processors.ProxyCoefficientCalculator(_settings);

#else
            coefCalculator = new Processors.ProxyCoefficientCalculator(_settings);
#endif
        }

        //[HttpGet]
        //[Route("change")]
        //public async Task<IActionResult> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        //{
        //    string result;

        //    try
        //    {
        //        result = await $"{_settings.BoxOptionsApi.CoefApiUrl}/change"
        //            .SetQueryParams(new
        //            {
        //                pair,
        //                timeToFirstOption,
        //                optionLen,
        //                priceSize,
        //                nPriceIndex,
        //                nTimeIndex
        //            })
        //            .GetStringAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }

        //    return Ok(result);
        //}
        [HttpGet]
        [Route("change")]
        public async Task<IActionResult> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            try
            {
                var result = await coefCalculator.ChangeAsync(pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        //[HttpGet]
        //[Route("request")]
        //public async Task<IActionResult> RequestAsync(string pair)
        //{
        //    string result;
        //    try
        //    {
        //        result = await $"{_settings.BoxOptionsApi.CoefApiUrl}/request"
        //            .SetQueryParams(new {pair})
        //            .GetStringAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }

        //    return Ok(result);
        //}
        [HttpGet]
        [Route("request")]
        public async Task<IActionResult> RequestAsync(string pair)
        {
            try
            {
                var result = await coefCalculator.RequestAsync(pair);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
