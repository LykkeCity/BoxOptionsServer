using Lykke.AzureQueueIntegration;

namespace BoxOptions.Common.Settings
{
    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; } = new AzureQueueSettings();
    }
}
