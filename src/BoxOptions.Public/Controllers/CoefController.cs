using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Core.Repositories;
using Common.Log;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class CoefController : Controller
    {
        private readonly BoxOptionsApiSettings _settings;
        ICoefficientCalculator coefCalculator;
        private readonly ILogRepository logRepository;
        private readonly ILog log;
        private readonly IAssetDatabase history;

        public CoefController(BoxOptionsApiSettings settings, IAssetDatabase history, ILogRepository logRepository, ILog log, ICoefficientCalculator coefCalculator)
        {
            _settings = settings;
            this.logRepository = logRepository;
            this.history = history;
            this.log = log;
            this.coefCalculator = coefCalculator;
            
        }

        /*[HttpGet]
        [Route("change")]
        public async Task<IActionResult> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex, string userId="0")
        {
            return StatusCode(500, "Request Obsolete");
            //try
            //{
            //    // Validate Parameters
            //    bool ValidateParameters = coefCalculator.ValidateChange(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            //    if (ValidateParameters == false)
            //    {
            //        // Invalid Parameters, report to logger and return Internal Server Error Code
            //        return StatusCode(500, "Invalid Parameters");
            //    }

            //    var result = await coefCalculator.ChangeAsync(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);

            //    bool ValidateResult = coefCalculator.ValidateChangeResult(result);
            //    if (ValidateResult == false)
            //    {
            //        // Invalid result
            //        return StatusCode(500, "Invalid Result");
            //    }
            //    else
            //    {

            //        await logRepository.InsertAsync(new LogItem
            //        {
            //            ClientId = userId,
            //            EventCode = ((int)(BoxOptionEvent.BOEventCoefChange)).ToString(),
            //            Message = $"[{pair}];timeToFirstOption={timeToFirstOption};optionLen={optionLen};priceSize={priceSize};nPriceIndex={nPriceIndex};nTimeIndex={nTimeIndex};Result=[{result}]"
            //        });
            //        return Ok(result);
            //    }
            //}
            //catch (Exception ex) { return StatusCode(500, ex.Message); }
        }*/

        [HttpGet]
        [Route("request")]
        public async Task<IActionResult> RequestAsync(string pair, string userId = "0")
        {
            try
            {
                string result = await coefCalculator.RequestAsync(userId, pair);
                return Ok(result);
            }
            catch (System.Exception ex) { return StatusCode(500, ex.Message); }
        }

        //[HttpGet]
        //[Route("history")]
        //public async Task<IActionResult> History(DateTime dtFrom, DateTime dtTo, string assetPair)
        //{
        //    if (history != null)
        //    {
        //        try
        //        {
        //            Core.AssetQuote[] res = null;
        //            var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
        //            res = new Core.AssetQuote[his.Count];
        //            if (res.Length > 0)
        //            {
        //                his.CopyTo(res, 0);
        //                var bidhistory = AssetBidProcessor.CreateBidHistory(assetPair, res);
        //                return Ok(bidhistory);
        //            }
        //            else
        //                return Ok("history is empty");
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, ex.Message);
        //        }
        //    }
        //    else
        //        return StatusCode(500, "History Not Available");

        //}
    }
}
