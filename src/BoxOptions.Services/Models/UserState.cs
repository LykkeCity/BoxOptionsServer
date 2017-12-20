using BoxOptions.Common;
using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using WampSharp.V2.Realm;

namespace BoxOptions.Services.Models
{
    public class UserState : IDisposable
    {
        private readonly string _userId;
        private readonly List<UserHistoryItem> _statusHistory;
        private readonly List<GameBet> _openBets;   // Bet cache

        private decimal balance;
        private GameStatus currentState;
        private ISubject<GameEvent> subject;    // WAMP Subject for publishing User Events
        
        public UserState(string userId)
        {
            _userId = userId;
            _statusHistory = new List<UserHistoryItem>();
            _openBets = new List<GameBet>();
            subject = null;
            LastChange = DateTime.UtcNow;
        }
        public UserState(IUserItem userItem)
            :this(userItem.UserId)
        {
            balance = decimal.Parse(userItem.Balance, System.Globalization.CultureInfo.InvariantCulture);
            LastChange = userItem.LastChange;
            currentState = (GameStatus)userItem.CurrentState;
        }

        /// <summary>
        /// Unique User Id
        /// </summary>
        public string UserId { get => _userId; }
        /// <summary>
        /// User Balance
        /// </summary>
        public decimal Balance { get => balance; }
        public GameStatus CurrentState { get => currentState; }
        public UserHistoryItem[] StatusHistory => _statusHistory.ToArray();
        public DateTime LastChange { get; set; }
        public GameBet[] OpenBets  { get => _openBets.ToArray(); }
    

        public void SetBalance(decimal newBalance)
        {
            balance = newBalance;
            LastChange = DateTime.UtcNow;

            PublishToWamp(new GameEvent()
            {
                EventType = (int)GameEventType.BalanceChanged,
                EventParameters = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", balance)
            });

        }
        internal UserHistoryItem SetStatus(GameStatus gameStatus, string message, double accountDelta = 0)
        {
            var newEntry = new UserHistoryItem
            {
                UserId = _userId,
                Date = DateTime.UtcNow,
                Message = message,
                AccountDelta = accountDelta,
                GameStatus = gameStatus
            };
            _statusHistory.Add(newEntry);

            // Keep load history buffer to 20 items
            if (_statusHistory.Count > 20)
            {
                _statusHistory.RemoveAt(0);
            }

            currentState = gameStatus;
            LastChange = DateTime.UtcNow;
            
            return newEntry;
        }

        internal GameBet PlaceBet(Box boxObject, string assetPair, decimal bet, BoxSize boxConfig)
        {
            GameBet retval = new GameBet(this)
            {
                AssetPair = assetPair,
                BetAmount = bet,
                BetStatus = BetStates.Waiting,
                Box = boxObject,
                CurrentParameters = boxConfig,
                Timestamp = DateTime.UtcNow
            };
            _openBets.Add(retval);
            // keep bet cache to 1000
            if (_openBets.Count > 1000)
                _openBets.RemoveAt(0);

            return retval;
        }
       
        internal void StartWAMP(IWampHostedRealm wampRealm, string topicName)
        {
            subject = wampRealm.Services.GetSubject<GameEvent>(topicName + "." + _userId);            
        }
        internal void PublishToWamp(GameEvent gameEvent)
        {
            if (subject == null)
                throw new InvalidOperationException("Wamp Subject not set");

            Console.WriteLine("PublishToWamp > {0}", gameEvent.EventTypeEnum);
            subject.OnNext(gameEvent);
        }

        public void Dispose()
        {
            subject = null;

            foreach (var bet in _openBets)
            {
                bet.Dispose();
            }
        }

       

        public override string ToString()
        {
            return _userId;
        }
    }
}