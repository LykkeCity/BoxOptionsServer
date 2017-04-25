using System.Text;
using Lykke.RabbitMqBroker.Subscriber;
using Newtonsoft.Json;

namespace BoxOptions.Common
{
    public class MessageDeserializer<T> : IMessageDeserializer<T>
    {
        public T Deserialize(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
