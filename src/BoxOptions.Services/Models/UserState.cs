using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class UserState
    {
        readonly string userId;
        decimal balance;

        List<StateHistory> statusHistory;
        int currentState;
        Game currentGame;

        // Coefficient Calculator parameters
        Dictionary<string, CoeffParameters> userCoeffParameters;

        public UserState(string userId)
        {
            this.userId = userId;
            statusHistory = new List<StateHistory>();
            currentGame = null;
            userCoeffParameters = new Dictionary<string, CoeffParameters>();
        }

        /// <summary>
        /// Unique User Id
        /// </summary>
        public string UserId { get => userId; }
        /// <summary>
        /// User Balance
        /// </summary>
        public decimal Balance { get => balance; }

        
        

        public StateHistory[] StatusHistory { get => statusHistory.ToArray(); }
        public int CurrentState { get => currentState; }
        public Game CurrentGame { get => currentGame; }
        public string CurrentGameId { get; set; }

        public void SetParameters(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            if (!userCoeffParameters.ContainsKey(pair))
                userCoeffParameters.Add(pair, new CoeffParameters());

            CoeffParameters pairParameters = userCoeffParameters[pair];
            pairParameters.TimeToFirstOption = timeToFirstOption;
            pairParameters.OptionLen = optionLen;
            pairParameters.PriceSize = priceSize;
            pairParameters.NPriceIndex = nPriceIndex;
            pairParameters.NTimeIndex = nTimeIndex;
        }

        public CoeffParameters GetParameters(string pair)
        {
            if (!userCoeffParameters.ContainsKey(pair))
                userCoeffParameters.Add(pair, new CoeffParameters());

            CoeffParameters pairParameters = userCoeffParameters[pair];
            return pairParameters;
        }
        
        public void SetBalance(decimal newBalance)
        {
            balance = newBalance;
        }

        internal void SetStatus(int status)
        {
            statusHistory.Add(new StateHistory()
            {
                Timestamp = DateTime.UtcNow,
                Status = status
            });
            currentState = status;
        }

        internal void SetGame(Game game)
        {
            if (currentGame == null)
            {
                currentGame = game;
                CurrentGameId = game.GameId;
            }
            else
                throw new InvalidOperationException("Current game not null");
        }

        internal void RemoveGame()
        {
            if (currentGame != null)
            {
                currentGame = null;
                CurrentGameId = null;
            }
            else
                throw new InvalidOperationException("User has no game ongoing.");
        }
    }
    public class StateHistory
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }

        public override string ToString()
        {
            return string.Format("{0} > {1}-{2}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), Status, (GameManager.GameStatus)Status);
        }
    }
}
