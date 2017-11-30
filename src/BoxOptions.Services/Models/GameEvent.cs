using Newtonsoft.Json;

namespace BoxOptions.Services.Models
{
    public class GameEvent
    {
        private int eventType;
        private string eventParameters;

        public GameEvent()
        {
            eventType = (int)GameEventType.Error;
            eventParameters = "";
        }

        [JsonIgnore]
        public GameEventType EventTypeEnum { get => (GameEventType)eventType; }
        public string EventParameters { get => eventParameters; set => eventParameters = value; }
        public int EventType { get => eventType; set => eventType = value; }
    }
}
