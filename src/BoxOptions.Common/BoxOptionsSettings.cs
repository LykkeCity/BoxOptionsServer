using Lykke.AzureQueueIntegration;

namespace BoxOptions.Common
{
    public class BoxOptionsSettings
    {
        public SlackNotificationsSettings SlackNotifications { get; set; } = new SlackNotificationsSettings();
        public BoxOptionsApiSettings BoxOptionsApi { get; set; } = new BoxOptionsApiSettings();
    }

    public class BoxOptionsApiSettings
    {
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new ConnectionStringsSettings();
        public PricesSettings PricesSettings { get; set;}
        public string CoefApiUrl { get; set; }
    }

    public class ConnectionStringsSettings
    {
        public string BoxOptionsApiStorage { get; set; }
        public string LogsConnString { get; set; }
    }

    public class PricesSettings
    {
        public string RabbitMqConnectionString { get; set; }
        public string RabbitMqExchangeName { get; set; }
        public string RabbitMqQueueName { get; set; }
        public bool RabbitMqIsDurable { get; set; }
        public string RabbitMqRoutingKey { get; set; }
        public string PricesTopicName { get; set; }
        public int GraphPointsCount { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; } = new AzureQueueSettings();
    }
}
