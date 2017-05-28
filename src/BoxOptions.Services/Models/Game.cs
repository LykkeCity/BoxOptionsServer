using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services.Models
{
    public class Game
    {
        readonly string assetPair;
        readonly string gameId;

        
        DateTime startDate;
        DateTime closeDate;


        //List<GameParametersHistory> parameterHistory;

        /// <summary>
        /// Key = boxId
        /// Valeu = BoxBet List
        /// </summary>
        List<GameBet> betList;

        public Game(string assetPair, string gameId)
        {
            this.gameId = gameId;
            this.assetPair = assetPair;
            startDate = DateTime.UtcNow;
            closeDate = DateTime.MaxValue;
            betList = new List<GameBet>();
            
            //parameterHistory = new List<GameParametersHistory>();

        }
        public string GameId => gameId;
        public string AssetPair { get => assetPair;  }
        public DateTime CreationDate { get => startDate; set => startDate = value; }
        public DateTime FinishDate { get => closeDate; set => closeDate = value; }
        

        internal GameBet PlaceBet(Box box, decimal betAmount, CoeffParameters parameters)
        {
            GameBet newBet = new GameBet()
            {
                Timestamp = DateTime.UtcNow,
                Box = box,
                BetAmount = betAmount,
                CurrentParameters = parameters
            };
            betList.Add(newBet);

            newBet.StartTimer();


            // TODO: Monitor Bet

            return newBet;
        }

        internal void LoadBets(IEnumerable<GameBet> bets)
        {
            // clear current list
            betList = new List<GameBet>();
            betList.AddRange(bets);
        }
    }
    
}
