namespace BoxOptions.Core
{
    public class BoxOptionsSettings
    {
        public string RabbitMqConnectionString { get; set; }
        public string RabbitMqExchangeName { get; set; }
        public string RabbitMqQueueName { get; set; }
        public bool RabbitMqIsDurable { get; set; }
        public string RabbitMqRoutingKey { get; set; }
        public string PricesTopicName { get; set; }
        public int GraphPointsCount { get; set; }

    }
}
