using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class UserState
    {

        readonly string  userId;
        decimal balance;

        List<StateHistory> statusHistory;
        int currentState;
        Game currentGame;

        public UserState(string userId)
        {
            this.userId = userId;
            statusHistory = new List<StateHistory>();
            currentGame = null;
        }


        public string UserId { get => userId; }
        public decimal Balance { get => balance; set => balance = value; }
        public StateHistory[] StatusHistory { get => statusHistory.ToArray(); }
        public int CurrentState { get => currentState; }

        public Game CurrentGame { get => currentGame; }
        public string CurrentGameId { get; set; }

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
