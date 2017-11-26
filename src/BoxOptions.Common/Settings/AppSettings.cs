namespace BoxOptions.Common.Settings
{
    public class AppSettings
    {
        public SlackNotificationsSettings SlackNotifications { get; set; } = new SlackNotificationsSettings();
        public BoxOptionsApiSettings BoxOptionsApi { get; set; } = new BoxOptionsApiSettings();
    }
}
