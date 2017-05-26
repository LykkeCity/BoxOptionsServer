using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameManager
    {
       
        void Launch(string userId);

        void Wake(string userId);
        void Sleep(string userId);

        /// <summary>
        /// Starts a game for the given user with the given assetpair.
        /// Returns gameId string
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="assetPair">Instrument</param>
        /// <returns>GameId Unique Identification</returns>
        string GameStart(string userId, string assetPair);

        /// <summary>
        /// Closes the running Game with given game id
        /// return "OK" or error message
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>"OK" or error message</returns>
        void GameClose(string userId);

        /// <summary>
        /// Place a bet on a box.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box in which to bet</param>
        /// <param name="bet">Bet ammount</param>
        /// <returns></returns>
        void PlaceBet(string userId, string box, decimal betAmount);

        void ChangeBet(string userId, string box, decimal betAmount);

        void ChangeScale(string userId, decimal scale);

        decimal SetUserBalance(string userId, decimal newBalance);

        decimal GetUserBalance(string userId);

        void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        string RequestUserCoeff(string userId, string pair);


        event EventHandler<BoxEventArgs> BetWin;

        event EventHandler<BoxEventArgs> BetLose;
    }
}
