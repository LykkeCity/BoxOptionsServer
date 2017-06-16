using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Models;
using Common.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

                    string HistoryRequestLog = string.Format("BidHistory> From:[{0}] To:[{1}] Pair:[{2}]", dtFrom.Date.ToString("yyyy-MM-dd"), dtTo.Date.ToString("yyyy-MM-dd"), assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Get Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                    var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
                    HistoryRequestLog += string.Format("\n\r{0}>Finished Getting Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));

                    if (his.Count > 0)
                    {
                        HistoryRequestLog += string.Format("\n\r{0}>Creating Bid History", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                        LinkedList<Price> bidhistory = new LinkedList<Price>(
                            from b in his
                            select new Price()
                            {
                                Ask = b.BestAsk.Value,
                                Bid = b.BestBid.Value,
                                Date = b.Timestamp
                            });
                        HistoryRequestLog += string.Format("\n\r{0}>Finished Creating Bid History", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
                        Console.WriteLine(HistoryRequestLog);
                        await appLog?.WriteInfoAsync("HistoryController", "BidHistory", null, HistoryRequestLog);

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


        /*
        [HttpGet]
        [Route("bidhistoryold")]
        public async Task<IActionResult> BidHistoryOld(DateTime dtFrom, DateTime dtTo, string assetPair)
        {

            if (history != null)
            {
                try
                {                    
                    AssetQuote[] res = null;
                    string HistoryRequestLog = string.Format("BidHistory> From:[{0}] To:[{1}] Pair:[{2}]", dtFrom.Date.ToString("yyyy-MM-dd"), dtTo.Date.ToString("yyyy-MM-dd"), assetPair);
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
        */


        //[HttpGet]
        //[Route("assethistory")]
        //public async Task<IActionResult> AssetHistory(DateTime dtFrom, DateTime dtTo, string assetPair)
        //{
        //    if (history != null)
        //    {
        //        try
        //        {
        //            string HistoryRequestLog = string.Format("AssetHistory> From:[{0}] To:[{1}] Pair:[{2}]", dtFrom.Date.ToString("yyyy-MM-dd"), dtTo.Date.ToString("yyyy-MM-dd"), assetPair);
        //            HistoryRequestLog += string.Format("\n\r{0}>Get Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
        //            var his = await history.GetAssetHistory(dtFrom, dtTo, assetPair);
        //            HistoryRequestLog += string.Format("\n\r{0}>Finished Getting Data From Azure", DateTime.UtcNow.ToString("HH:mm:ss.fff"));

        //            await appLog.WriteInfoAsync("HistoryController", "AssetHistory", null, HistoryRequestLog);

        //            return Ok(his);                    
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, ex.Message);
        //        }
        //    }
        //    else
        //        return StatusCode(500, "History Not Available");

        //}

        //[HttpGet]
        //[Route("migrate")]
        //public async Task<IActionResult> Migrate()
        //{
        //    return Ok();
        //    if (history != null)
        //    {
        //        try
        //        {
        //            string HistoryRequestLog = "<< Migrate Start >> ";
        //            Console.WriteLine(HistoryRequestLog);

        //            DateTime startdate = DateTime.Now.Date.AddDays(-2);
        //            DateTime enddate = DateTime.UtcNow;

        //            List<string> assetList = new List<string>();
        //            assetList.AddRange(settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.AllowedAssets);
        //            assetList.AddRange(settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.AllowedAssets);

        //            string[] allowedAssets = assetList.Distinct().ToArray();
        //            do
        //            {


        //                foreach (var assetPair in allowedAssets)
        //                {
        //                    // Read Old History
        //                    string histline = string.Format("{0}>Get Data From Old Database > [{1}] {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), assetPair, startdate.ToString("yyyy-MM.dd"));
        //                    HistoryRequestLog += "\n\r" + histline;
        //                    Console.WriteLine(histline);
        //                    var his = await oldrep.GetRange(startdate, startdate.AddDays(1), assetPair);

        //                    // Add to new history;
        //                    histline = string.Format("{0}>Insert Data to new DB, {1} items", DateTime.UtcNow.ToString("HH:mm:ss.fff"), his.Count());
        //                    HistoryRequestLog += "\n\r" + histline;
        //                    Console.WriteLine(histline);
        //                    await newrep.InsertManyAsync(his);

        //                    histline = string.Format("{0}>Finished Inserting", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
        //                    HistoryRequestLog += "\n\r" + histline;
        //                    Console.WriteLine(histline);

        //                }
        //                startdate = startdate.AddDays(1);

        //            } while (startdate < enddate);


        //            string histline1 = string.Format("{0}<< Migrate Finished >> ", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
        //            HistoryRequestLog += "\n\r" + histline1;
        //            Console.WriteLine(histline1);
        //            //await appLog.WriteInfoAsync("HistoryController", "AssetHistory", null, HistoryRequestLog);


        //            return Ok();
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
