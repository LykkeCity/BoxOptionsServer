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
        private readonly IAssetDatabase history;
        private readonly Services.Interfaces.IHistoryHolder histHolder;
        private readonly ILog appLog;
        


        public HistoryController(IAssetDatabase history, Services.Interfaces.IHistoryHolder histHolder, ILog appLog)
        {
            this.history = history;
            this.appLog = appLog;
            this.histHolder = histHolder;
        }
              
        [HttpGet]
        [Route("bidhistoryholder")]
        public async Task<IActionResult> BidHistoryHolder(string assetPair)
        {
            if (histHolder != null)
            {
                try
                {
                    List<Price> retval = await Task.Run(() => histHolder.GetHistory(assetPair).ToList());
                    return Ok(retval);
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
