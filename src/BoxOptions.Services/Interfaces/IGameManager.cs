using BoxOptions.Services.Models;
using System;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameManager
    {

        Core.Models.BoxSize[] InitUser(string userId);
        /// <summary>
        /// Place a bet on a box.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box in which to bet</param>
        /// <param name="bet">Bet ammount</param>
        /// <returns>Bet Timestamp</returns>
        DateTime PlaceBet(string userId, string assetpair, string box, decimal betAmount, out string message);

        decimal SetUserBalance(string userId, decimal newBalance);

        decimal GetUserBalance(string userId);

        void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        CoeffParameters GetUserParameters(string userId, string pair);
        string RequestUserCoeff(string userId, string pair);
        
        event EventHandler<BetEventArgs> BetWin;
        event EventHandler<BetEventArgs> BetLose;

        
    }
}
