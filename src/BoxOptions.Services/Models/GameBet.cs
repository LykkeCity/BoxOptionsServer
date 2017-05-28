using System;

namespace BoxOptions.Services.Models
{
    public class GameBet: IDisposable
    {
        public decimal BetAmount { get; set; }
        public DateTime Timestamp { get; set; }
        public Box Box { get; set; }
        public CoeffParameters CurrentParameters { get; set; }

        System.Threading.Timer BetTimer;
        public GameBet()
        {
            BetTimer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallback), Box, -1, -1);
        }
        

        public override string ToString()
        {
            return string.Format("{0} > {1:f4}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), BetAmount);
        }

        internal void StartTimer()
        {
            BetTimer.Change(1000 * Box.TimeToGraph, -1);
        }

        private void TimerCallback(object status)
        {
            TimeToGraphReached?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            BetTimer.Change(-1, -1);
            BetTimer.Dispose();
        }

        public event EventHandler TimeToGraphReached;

    }
}
