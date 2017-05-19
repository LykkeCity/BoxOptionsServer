using System;
namespace BoxOptions.Public.Models
{
    public class LogModel
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string EventDesc
        {
            get
            {
                int code = 0;
                if (int.TryParse(EventCode, out code))
                {
                    EventType etype = (EventType)code;
                    return etype.ToString();
                }
                else
                    return "Unknown Event Code";
            }
        }


        enum EventType
        {
            Launch = 1,
            Wake = 2,
            Sleep = 3,
            GameStarted = 4,
            GameClosed = 5,
            ChangeBet = 6,
            ChangeScale = 7,
            BetPlaced = 8,
            BetWon = 9,
            BetLost = 10,
            ServerRequest = 11,
            ServerChange = 12
        }

    }
}
