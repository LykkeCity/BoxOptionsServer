namespace BoxOptions.Common.Settings
{
    public class FeedSettings
    {
        public string RabbitMqConnectionString { get; set; }
        public string RabbitMqExchangeName { get; set; }
        public string RabbitMqQueueName { get; set; }

        public int IncomingDataCheckInterval { get; set; }
        public string PricesWeekExclusionStart { get; set; }
        public string PricesWeekExclusionEnd { get; set; }
        public string[] AllowedAssets { get; set; }
    }
}
