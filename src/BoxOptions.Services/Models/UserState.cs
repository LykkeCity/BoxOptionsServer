using BoxOptions.Common.Interfaces;
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

        List<UserHistory> statusHistory;
        int currentState;
        
        // Coefficient Calculator parameters
        List<CoeffParameters> userCoeffParameters;

        List<GameBet> openBets;

        public UserState(string userId)
        {            
            this.userId = userId;            
            statusHistory = new List<UserHistory>();            
            userCoeffParameters = new List<CoeffParameters>();
            openBets = new List<GameBet>();
            LastChange = DateTime.UtcNow;
        }
        public UserState(string userId, decimal balance, int currentState) 
            :this(userId)
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
        public List<GameBet> OpenBets { get => openBets; }
        public CoeffParameters[] UserCoeffParameters => userCoeffParameters.ToArray();
        public UserHistory[] StatusHistory => statusHistory.ToArray();
        public DateTime LastChange { get; set; }


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

        internal UserHistory SetStatus(int status, string message)
        {
            UserHistory newEntry = new UserHistory()
            {
                Timestamp = DateTime.UtcNow,
                Status = status,
                Message = message
            };
            statusHistory.Add(newEntry);

            // Keep load history buffer to 20 items
            if (statusHistory.Count > 20)
            {
                statusHistory.RemoveAt(0);
            }
            
            currentState = status;
            LastChange = DateTime.UtcNow;
            return newEntry;
        }
      

        internal GameBet PlaceBet(Box boxObject, string assetPair, decimal bet, CoeffParameters coefPars, IAssetQuoteSubscriber quoteFeed)
        {
            GameBet retval = new GameBet(userId)
            {
                AssetPair = assetPair,
                BetAmount = bet,
                BetStatus = GameBet.BetStates.Waiting,
                Box = boxObject,
                CurrentParameters = coefPars,
                Timestamp = DateTime.UtcNow
            };            
            openBets.Add(retval);
            return retval;
        }
        internal void LoadBets(IEnumerable<GameBet> bets)
        {
            openBets = new List<GameBet>(); ;
            openBets.AddRange(bets);
        }

       
    }
}
