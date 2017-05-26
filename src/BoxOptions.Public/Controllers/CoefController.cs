using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core;
using BoxOptions.Core.Models;
using Common.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class CoefController : Controller
    {
        private readonly BoxOptionsSettings _settings;
        ICoefficientCalculator coefCalculator;
        private readonly ILogRepository logRepository;
        private readonly ILog log;
        private readonly IBoxOptionsHistory history;

        public CoefController(BoxOptionsSettings settings, IBoxOptionsHistory history, ILogRepository logRepository, ILog log, ICoefficientCalculator coefCalculator)
        {
            _settings = settings;
            this.logRepository = logRepository;
            this.history = history;
            this.log = log;
            this.coefCalculator = coefCalculator;
            
        }
      
        [HttpGet]
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
        }

        [HttpGet]
        [Route("request")]
        public async Task<IActionResult> RequestAsync(string pair, string userId = "0")
        {
            return StatusCode(500, "Request Obsolete");
            //try
            //{
            //    // Validate parameters
            //    bool parOk = coefCalculator.ValidateRequest(userId, pair);
            //    if (!parOk)
            //    {
            //        // Invalid Parameters, report to logger and return Internal Server Error Code
            //        return StatusCode(500, "Invalid Parameters");
            //    }
            //    else
            //    {
            //        string result = await coefCalculator.RequestAsync(userId, pair);
            //        bool resOk = true; ;
            //        try { resOk = coefCalculator.ValidateRequestResult(result); }
            //        catch (Exception ex)
            //        {
            //            // If all coefficients are equal to 1.0
            //            // return answer to client.
            //            // https://lykkex.atlassian.net/browse/BOXOPT-12?focusedCommentId=26511&page=com.atlassian.jira.plugin.system.issuetabpanels%3Acomment-tabpanel#comment-26511
            //            if (ex.Message == "All coefficients are equal to 1.0")
            //                resOk = true;
            //            // If there are negative coefficients
            //            // return Error
            //            else if (ex.Message == "Negative coefficients")
            //                resOk = false;
            //            else
            //                resOk = false;
            //            await log.WriteErrorAsync("Coef/Request", $"pair={pair}&userId={userId}", "", ex);
            //        }
            //        if (!resOk)
            //        {
            //            // Invalid result
            //            return StatusCode(500, "Invalid Result");
            //        }
            //        else
            //        {
            //            await logRepository.InsertAsync(new LogItem
            //            {
            //                ClientId = userId.ToString(),
            //                EventCode = ((int)(BoxOptionEvent.BOEventCoefRequest)).ToString(),
            //                Message = $"[{pair}];Result=[{result}]"
            //            });
            //            return Ok(result);
            //        }
            //    }
            //}
            //catch (Exception ex) { return StatusCode(500, ex.Message); }
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
