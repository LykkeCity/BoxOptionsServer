using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Game
    {
        readonly string assetPair;
        readonly string gameId;

        //int currentStatus;
        DateTime creationDate;
               

        //List<GameParametersHistory> parameterHistory;

        /// <summary>
        /// Key = boxId
        /// Valeu = BoxBet List
        /// </summary>
        List<BoxBet> betList;

        public Game(string assetPair, string gameId)
        {
            this.gameId = gameId;
            this.assetPair = assetPair;
            creationDate = DateTime.UtcNow;
            betList = new List<BoxBet>();
            //parameterHistory = new List<GameParametersHistory>();

        }
        public string GameId => gameId;
        public string AssetPair { get => assetPair;  }



        internal void PlaceBet(Box box, decimal betAmount)
        {
            BoxBet newBet = new BoxBet()
            {
                Timestamp = DateTime.UtcNow,
                Box = box,
                BetAmount = betAmount
            };
            betList.Add(newBet);
        }
    }
    
}
