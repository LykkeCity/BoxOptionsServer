using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using System.Collections.Generic;
using WampSharp.V2.Rpc;

namespace BoxOptions.Common.Interfaces
{
    public interface IRpcMethods
    {
        /// <summary>
        /// Client calls init.chartdata RPC to get that data for charts
        /// </summary>
        /// <returns></returns>
        [WampProcedure("init.chartdata")]
        Dictionary<string, Price[]> InitChartData();

        /// <summary>
        /// Client calls init.assets RPC method to get list of asset pairs
        /// </summary>
        /// <returns></returns>
        [WampProcedure("init.assets")]
        AssetPair[] InitAssets();

        /// <summary>
        /// Initialize user WAMP topic if not already running
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>OK or error message</returns>
        [WampProcedure("user.init")]
        string InitUser(string userId);

        /// <summary>
        /// Get current user's balance
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>User's balance</returns>
        [WampProcedure("user.getbalance")]
        decimal GetBalance(string userId);

        /// <summary>
        /// Set User Balance
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="balance">New Balance</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("user.setbalance")]
        string SetBalance(string userId, decimal balance);



        /// <summary>
        /// Bet Placed
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="assetPair">Asset pair of the bet</param>
        /// <param name="box">Box Details(json)</param>
        /// <param name="betValue">Bet Value</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("game.placebet")]
        IPlaceBetResult PlaceBet(string userId, string assetPair, string box, decimal betValue);

        /// <summary>
        /// Saves log to database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [WampProcedure("game.savelog")]
        string SaveLog(string userId, string eventCode, string message);


        /// <summary>
        /// Coefficient Api Request Coefficients
        /// </summary>
        /// <param name="userId">User If</param>
        /// <param name="pair">Instrument</param>
        /// <returns>Coefficient array(json) or error string</returns>
        [WampProcedure("coeffapi.requestcoeff")]
        string RequestCoeff(string userId, string pair);


    }
}
