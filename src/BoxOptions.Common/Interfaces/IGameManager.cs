using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IGameManager
    {


        IBoxSize[] InitUser(string userId);

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


        //void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        //CoeffParameters GetUserParameters(string userId, string pair);
        string RequestUserCoeff(string pair, string userId = null);

        void AddUserLog(string userId, string eventCode, string message);

        Task SetBoxConfig(IBoxSize[] boxes);
        
    }
}
