using BoxOptions.Common;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class CoefController : Controller
    {
        private readonly BoxOptionsSettings _settings;
        Processors.ICoefficientCalculator coefCalculator;
        private readonly ILogRepository logRepository;
        private readonly IBoxOptionsHistory history;

        public CoefController(BoxOptionsSettings settings, IBoxOptionsHistory history, ILogRepository logRepository)
        {
            _settings = settings;
            this.logRepository = logRepository;
            this.history = history;
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

                await logRepository.InsertAsync(new LogItem
                {
                    ClientId = userId,
                    EventCode = ((int)(BoxOptionEvent.BOEventCoefChange)).ToString(),
                    Message = $"[{pair}];timeToFirstOption={timeToFirstOption};optionLen={optionLen};priceSize={priceSize};nPriceIndex={nPriceIndex};nTimeIndex={nTimeIndex};Result=[{result}]"
                });
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
        
        [HttpGet]
        [Route("request")]
        public async Task<IActionResult> RequestAsync(string pair, string userId = "0")
        {
            try
            {   var result = await coefCalculator.RequestAsync(pair, userId);                
                await logRepository.InsertAsync(new LogItem
                {
                    ClientId = userId,
                    EventCode = ((int)(BoxOptionEvent.BOEventCoefRequest)).ToString(),
                    Message = $"[{pair}];Result=[{result}]"
                });
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet]
        [Route("history")]
        public async Task<IActionResult> History(DateTime dtFrom, DateTime dtTo, string assetPair)
        {
            if (history != null)
            {
                Core.AssetQuote[] res = null;
                var his = await history.GetAssetHistory(dtFrom,dtTo, assetPair);
                res = new Core.AssetQuote[his.Count];
                his.CopyTo(res, 0);                
                return Ok(res);
            }
            else
                return StatusCode(500, "History Not Available");

        }
    }
}
