using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameManager
    {

        void InitUser(string userId);
        /// <summary>
        /// Place a bet on a box.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box in which to bet</param>
        /// <param name="bet">Bet ammount</param>
        /// <returns>Bet Timestamp</returns>
        DateTime PlaceBet(string userId, string assetpair, string box, decimal betAmount);

        decimal SetUserBalance(string userId, decimal newBalance);

        decimal GetUserBalance(string userId);

        void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        CoeffParameters GetUserParameters(string userId, string pair);
        string RequestUserCoeff(string userId, string pair);

        void AddLog(string userId, string eventCode, string message);
        
        event EventHandler<BetEventArgs> BetWin;
        event EventHandler<BetEventArgs> BetLose;

        
    }
}
