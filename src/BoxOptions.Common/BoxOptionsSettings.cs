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
        public PricesSettingsBoxOptions PricesSettingsBoxOptions { get; set; }
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

    public class PricesSettingsBoxOptions
    {
        public string RabbitMqMicrographConnectionString { get; set; }
        public string RabbitMqMicrographExchangeName { get; set; }
        public string RabbitMqMicrographRoutingKey { get; set; }        

        public string RabbitMqPricesConnectionString { get; set; }
        public string RabbitMqPricesExchangeName { get; set; }
        public string RabbitMqPricesQueueName { get; set; }
        public string RabbitMqPricesRoutingKey { get; set; }
        public bool RabbitMqPricesIsDurable { get; set; }

        public string PricesTopicName { get; set; }
        public int GraphPointsCount { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; } = new AzureQueueSettings();
    }
}
