using BoxOptions.Core.Models;
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
        /// Game Started
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="assetPair">AssetPair of the new game</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("game.start")]
        string GameStart(string userId, string assetPair);


        /// <summary>
        /// Game Closed
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("game.close")]
        string GameClose(string userId);

        /// <summary>
        /// Bet Placed
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box Details(json)</param>
        /// <param name="betValue">Bet Value</param>
        /// <returns>'OK' or error string</returns>
        [WampProcedure("game.placebet")]
        string PlaceBet(string userId, string box, decimal betValue);

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
        /// Retrieve Coefficient Api Parameters
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="pair">Asset Pair</param>
        /// <returns></returns>
        [WampProcedure("coeffapi.getparameters")]
        Models.CoeffParameters GetParameters(string userId, string pair);

        /// <summary>
        /// Coefficient Api Request Coefficients
        /// </summary>
        /// <param name="userId">User If</param>
        /// <param name="pair">Instrument</param>
        /// <returns>Coefficient array(json) or error string</returns>
        [WampProcedure("coeffapi.requestcoeff")]
        string RequestCoeff(string userId, string pair);



        /// <summary>
        /// Application Launch
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>'OK' or error string</returns>
        //[WampProcedure("game.launch")]
        //string Launch(string userId);

        /// <summary>
        /// Application Wake Up
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>'OK' or error string</returns>
        //[WampProcedure("game.wake")]
        //string Wake(string userId);

        /// <summary>
        /// Application Sleep
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>'OK' or error string</returns>
        //[WampProcedure("game.sleep")]
        //string Sleep(string userId);

        /// <summary>
        /// Bet Changed
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box Details(json)</param>
        /// <param name="betValue">Bet Value</param>
        /// <returns>'OK' or error string</returns>
        //[WampProcedure("game.changebet")]
        //string ChangeBet(string userId, string box, decimal betValue);

        /// <summary>
        /// Scale Changed
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="scale">Scale</param>
        /// <returns>'OK' or error string</returns>
        //[WampProcedure("game.changescale")]
        //string ChangeScale(string userId, decimal scale);

    }
}
