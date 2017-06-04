using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Models;
using Common.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class HistoryController: Controller
    {
        IAssetDatabase history;
        ILog appLog;

        public HistoryController(IAssetDatabase history, ILog appLog)
        {
            this.history = history;
            this.appLog = appLog;
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
                    string HistoryRequestLog = string.Format("BidHistory> From:[{0} To:[{1}] Pair:[{2}]", dtFrom.Date, dtTo.Date, assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Get Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                    var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Finished Getting Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                    res = new AssetQuote[his.Count];
                    if (res.Length > 0)
                    {
                        his.CopyTo(res, 0);
                        HistoryRequestLog += string.Format("\n\r{0}>Creating Bid History", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                        var bidhistory = AssetBidProcessor.CreateBidHistory(assetPair, res);
                        HistoryRequestLog += string.Format("\n\r{0}>Finished Creating Bid History", DateTime.UtcNow.ToString("HH:mm:ss.fff"));

                        await appLog.WriteInfoAsync("HistoryController", "BidHistory", null, HistoryRequestLog);

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
                    string HistoryRequestLog = string.Format("AssetHistory> From:[{0} To:[{1}] Pair:[{2}]", dtFrom.Date, dtTo.Date, assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Get Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                    var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Finished Getting Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));

                    await appLog.WriteInfoAsync("HistoryController", "AssetHistory", null, HistoryRequestLog);

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
