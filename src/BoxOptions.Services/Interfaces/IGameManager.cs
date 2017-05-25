using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameManager
    {
       
        string Launch(string userId);

        string Wake(string userId);
        string Sleep(string userId);

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
        string GameClose(string userId);

        /// <summary>
        /// Place a bet on a box.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="box">Box in which to bet</param>
        /// <param name="bet">Bet ammount</param>
        /// <returns></returns>
        string PlaceBet(string userId, string box, decimal betAmount);
        
        string ChangeBet(string userId, string box, decimal betAmount);

        string ChangeScale(string userId, decimal scale);

        event EventHandler<BoxEventArgs> BetWin;

        event EventHandler<BoxEventArgs> BetLose;
    }
}
