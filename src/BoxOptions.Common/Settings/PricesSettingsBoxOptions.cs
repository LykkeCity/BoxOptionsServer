namespace BoxOptions.Common.Settings
{
    public class PricesSettingsBoxOptions
    {
        public FeedSettings PrimaryFeed { get; set; }
        public FeedSettings SecondaryFeed { get; set; } = null;
        public string PricesTopicName { get; set; }
        public int GraphPointsCount { get; set; }
        public int NoFeedSlackReportInSeconds { get; set; }
    }
}
