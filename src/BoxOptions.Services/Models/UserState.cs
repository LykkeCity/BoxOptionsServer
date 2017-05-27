using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


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
        List<CoeffParameters> userCoeffParameters;

        public UserState(string userId)
        {
            this.userId = userId;
            statusHistory = new List<StateHistory>();
            currentGame = null;
            userCoeffParameters = new List<CoeffParameters>();
            LastChange = DateTime.UtcNow;
        }
        public UserState(string userId, decimal balance, int currentState):this(userId)
        {
            this.balance = balance;            
            this.currentState = currentState;
        }
                
                
        /// <summary>
        /// Unique User Id
        /// </summary>
        public string UserId { get => userId; }
        /// <summary>
        /// User Balance
        /// </summary>
        public decimal Balance { get => balance; }




        
        public int CurrentState { get => currentState; }
        public Game CurrentGame { get => currentGame; }
        public string CurrentGameId { get; set; }
        public DateTime LastChange { get; set; }

        public CoeffParameters[] UserCoeffParameters => userCoeffParameters.ToArray();
        public StateHistory[] StatusHistory => statusHistory.ToArray();


        public void SetParameters(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {

            CoeffParameters selectedPair = (from c in userCoeffParameters
                                           where c.AssetPair == pair
                                           select c).FirstOrDefault();
            // Pair does not exist on parameter list, Add It
            if (selectedPair == null)
            {
                selectedPair = new CoeffParameters() { AssetPair = pair };
                userCoeffParameters.Add(selectedPair);
            }
            // Set parameters
            selectedPair.TimeToFirstOption = timeToFirstOption;
            selectedPair.OptionLen = optionLen;
            selectedPair.PriceSize = priceSize;
            selectedPair.NPriceIndex = nPriceIndex;
            selectedPair.NTimeIndex = nTimeIndex;
            
            LastChange = DateTime.UtcNow;
        }
        public void LoadParameters(IEnumerable<CoeffParameters> pars)
        {
            // Ensure no duplicates
            var distictPairs = (from p in pars
                                select p.AssetPair).Distinct();
            if (distictPairs.Count() != pars.Count())
                throw new ArgumentException("Duplicate Assets found");


                userCoeffParameters = new List<CoeffParameters>(pars);
        }

        public CoeffParameters GetParameters(string pair)
        {
            CoeffParameters selectedPair = (from c in userCoeffParameters
                                            where c.AssetPair == pair
                                            select c).FirstOrDefault();
            // Pair does not exist on parameter list, Add It
            if (selectedPair == null)
            {
                selectedPair = new CoeffParameters() { AssetPair = pair };
                userCoeffParameters.Add(selectedPair);
            }

            return selectedPair;
        }
        
        public void SetBalance(decimal newBalance)
        {
            balance = newBalance;
            LastChange = DateTime.UtcNow;
        }

        internal void SetStatus(int status)
        {
            statusHistory.Add(new StateHistory()
            {
                Timestamp = DateTime.UtcNow,
                Status = status
            });
            currentState = status;
            LastChange = DateTime.UtcNow;
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

            LastChange = DateTime.UtcNow;
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

            LastChange = DateTime.UtcNow;
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
