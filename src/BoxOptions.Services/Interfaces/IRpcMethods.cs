using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using WampSharp.V2.Rpc;

namespace BoxOptions.Services
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
        Models.PlaceBetResult PlaceBet(string userId, string assetPair ,string box, decimal betValue);
        
        /// <summary>
        /// Coefficient Api Change Parameters
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="pair">Instrument</param>
        /// <param name="timeToFirstOption">timeToFirstOption</param>
        /// <param name="optionLen">optionLen</param>
        /// <param name="priceSize">priceSize</param>
        /// <param name="nPriceIndex">nPriceIndex</param>
        /// <param name="nTimeIndex">nTimeIndex</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("coeffapi.changeparameters")]
        string ChangeParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        

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
