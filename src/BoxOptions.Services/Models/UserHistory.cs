using System;


namespace BoxOptions.Services.Models
{
    public class UserHistory
    {
        string userId;

        public UserHistory(string userId)
        {
            this.userId = userId;
        }

        public string UserId => userId;
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public double AccountDelta { get; set; }

        public override string ToString()
        {
            return string.Format("{0} > {1}-{2}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), Status, (GameStatus)Status);
        }
    }
}
