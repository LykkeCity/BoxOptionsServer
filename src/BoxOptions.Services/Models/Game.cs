using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Game
    {
        readonly string assetPair;
        readonly string gameId;

        int currentStatus;
        DateTime creationDate;
        

        GameParameters parameters;

        List<GameParametersHistory> parameterHistory;

        /// <summary>
        /// Key = boxId
        /// Valeu = BoxBet List
        /// </summary>
        Dictionary<string, List<BoxBet>> betList;

        public Game(string assetPair, string gameId)
        {
            this.gameId = gameId;
            this.assetPair = assetPair;
            creationDate = DateTime.UtcNow;
            betList = new Dictionary<string, List<BoxBet>>();
            parameterHistory = new List<GameParametersHistory>();

        }
        public string GameId => gameId;
        public string AssetPair { get => assetPair;  }



        internal void PlaceBet(Box box, decimal bet)
        {
            if (!betList.ContainsKey(box.Id))
                betList.Add(box.Id, new List<BoxBet>());

            betList[box.Id].Add(new BoxBet() { Timestamp = DateTime.UtcNow, BetValue = bet });

        }

        class BoxBet
        {
            public decimal BetValue { get; set; }
            public DateTime Timestamp { get; set; }

            public override string ToString()
            {
                return string.Format("{0} > {1:f4}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), BetValue);
            }
        }
    }
    
}
