using BoxOptions.Common.Interfaces;
using System;

namespace BoxOptions.Services.Models
{
    public class GameBet : IDisposable
    {
        public enum BetStates
        {
            Waiting = 0,
            OnGoing = 1,
            Lose = 2,
            Win = 3
        }

        readonly string userId;
        UserState user;

        public string UserId => userId;
        public UserState User => user;
        public decimal BetAmount { get; set; }
        public string AssetPair { get; set; }
        public DateTime Timestamp { get; set; }
        public Box Box { get; set; }
        public BetStates BetStatus { get; set; }
        public CoeffParameters CurrentParameters { get; set; }
        public DateTime? TimeToGraphStamp { get; private set; }
        public DateTime? WinStamp { get; set; }
        public DateTime? FinishedStamp { get; private set; }

        

        System.Threading.Timer BetTimer;

        public GameBet(string userId)
        {
            this.userId = userId;
            BetTimer = new System.Threading.Timer(new System.Threading.TimerCallback(WaitTimeToGraphCallback), null, -1, -1);
            user = null;
            TimeToGraphStamp = null;
            WinStamp = null;
            FinishedStamp = null;
        }
        public GameBet(UserState user) :
            this(user.UserId)
        {
            this.user = user;
        }
        private void AssignUser(UserState user)
        {
            if (user != null)
                throw new InvalidOperationException("User already defined");
            this.user = user;
        }

        public override string ToString()
        {
            return string.Format("{0} > {1:f4}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), BetAmount);
        }        

        internal void StartWaitTimeToGraph()
        {
            BetStatus = BetStates.Waiting;
            BetTimer.Change((int)(1000 * Box.TimeToGraph), -1);
        }

        private void WaitTimeToGraphCallback(object status)
        {
            ClearTimer();
            TimeToGraphStamp = DateTime.UtcNow;
            BetStatus = BetStates.OnGoing;

            BetTimer = new System.Threading.Timer(new System.Threading.TimerCallback(WaitTimeLengthCallback), Box, (int)(1000 * Box.TimeLength), -1);

            TimeToGraphReached?.Invoke(this, new EventArgs());
        }
        private void WaitTimeLengthCallback(object status)
        {
            ClearTimer();
            FinishedStamp = DateTime.UtcNow;

            TimeLenghFinished?.Invoke(this, new EventArgs());
        }
        private void ClearTimer()
        {
            BetTimer.Dispose();
            BetTimer = null;

        }
        public void Dispose()
        {
            if (BetTimer != null)
            {
                BetTimer.Change(-1, -1);
                BetTimer.Dispose();
                BetTimer = null;
            }
        }

        public event EventHandler TimeToGraphReached;
        public event EventHandler TimeLenghFinished;

    }
}
