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
        }


        public string UserId { get => userId; }
        public decimal Balance { get => balance; set => balance = value; }
        public StateHistory[] StatusHistory { get => statusHistory.ToArray(); }
        public int CurrentState { get => currentState; }
        public Game CurrentGame { get => currentGame; }

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
                currentGame = game;
            else
                throw new InvalidOperationException("Current game not null");
        }
    }
    public class StateHistory
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
    }
}
