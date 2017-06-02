using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class HistoryController: Controller
    {
        IAssetDatabase history;

        public HistoryController(IAssetDatabase history)
        {
            this.history = history;
        }

        [HttpGet]
        [Route("bidhistory")]
        public async Task<IActionResult> BidHistory(DateTime dtFrom, DateTime dtTo, string assetPair)
        {
            if (history != null)
            {
                try
                {
                    AssetQuote[] res = null;
                    Console.WriteLine("{0}>Get Data From Azure", DateTime.Now.ToString("HH:mm:ss.fff"));
                    var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
                    Console.WriteLine("{0}>Finished Getting Data From Azure", DateTime.Now.ToString("HH:mm:ss.fff"));
                    res = new AssetQuote[his.Count];
                    if (res.Length > 0)
                    {
                        his.CopyTo(res, 0);
                        Console.WriteLine("{0}>Creating Bid History", DateTime.Now.ToString("HH:mm:ss.fff"));
                        var bidhistory = AssetBidProcessor.CreateBidHistory(assetPair, res);
                        Console.WriteLine("{0}>Finished Creating Bid History", DateTime.Now.ToString("HH:mm:ss.fff"));
                        return Ok(bidhistory);
                    }
                    else
                        return Ok("history is empty");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
                return StatusCode(500, "History Not Available");

        }

        [HttpGet]
        [Route("assethistory")]
        public async Task<IActionResult> AssetHistory(DateTime dtFrom, DateTime dtTo, string assetPair)
        {
            if (history != null)
            {
                try
                {
                    var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
                    return Ok(his);                    
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
                return StatusCode(500, "History Not Available");

        }
    }
}
