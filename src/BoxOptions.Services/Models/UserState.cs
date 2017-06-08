using BoxOptions.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using WampSharp.V2.Realm;

namespace BoxOptions.Services.Models
{
    public class UserState : IDisposable
    {
        readonly string userId;
        decimal balance;
        int currentState;
        List<UserHistory> statusHistory;
        List<CoeffParameters> userCoeffParameters;  // Coefficient Calculator parameters
        List<GameBet> openBets;                     // Bet cache
        ISubject<BetResult> subject;                // WAMP Subject

        private DateTime lastDbSync;
        public event EventHandler DbSyncRequest;
        System.Threading.Timer dbSyncTimer;

        public UserState(string userId)
        {
            this.userId = userId;
            statusHistory = new List<UserHistory>();
            userCoeffParameters = new List<CoeffParameters>();
            openBets = new List<GameBet>();
            subject = null;
            LastChange = DateTime.UtcNow;

            lastDbSync = DateTime.UtcNow;
            dbSyncTimer = new System.Threading.Timer(new System.Threading.TimerCallback(DbSyncTimerCallback), null, 20 * 1000, 20 * 1000);            
        }
        public UserState(string userId, decimal balance, int currentState)
            : this(userId)
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
            lastDbSync = DateTime.UtcNow;

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

        internal GameBet PlaceBet(Box boxObject, string assetPair, decimal bet, CoeffParameters coefPars)
        {
            GameBet retval = new GameBet(this)
            {
                AssetPair = assetPair,
                BetAmount = bet,
                BetStatus = GameBet.BetStates.Waiting,
                Box = boxObject,
                CurrentParameters = coefPars,
                Timestamp = DateTime.UtcNow
            };
            openBets.Add(retval);
            // keep bet cache to 100
            if (openBets.Count > 100)
                openBets.RemoveAt(0);

            return retval;
        }
        internal void LoadBets(IEnumerable<GameBet> bets)
        {
            openBets = new List<GameBet>(); ;
            openBets.AddRange(bets);
        }

        internal void StartWAMP(IWampHostedRealm wampRealm, string topicName)
        {
            subject = wampRealm.Services.GetSubject<BetResult>(topicName + "." + userId);
        }
        internal void PublishToWamp(BetResult betResult)
        {
            if (subject == null)
                throw new InvalidOperationException("Wamp Subject not set");

            subject.OnNext(betResult);
        }

        public void Dispose()
        {
            subject = null;

            foreach (var bet in openBets)
            {
                bet.Dispose();
            }
        }

        private void DbSyncTimerCallback(object status)
        {            
            if (LastChange < lastDbSync)
                return; // No changes since last save to db
            else
            {
             //   OnDbSyncRequest
            }
            

        }

        private void OnDbSyncRequest(EventArgs e)
        {
            DbSyncRequest?.BeginInvoke(this, e, null, null);
        }
    }
}
