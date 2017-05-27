namespace BoxOptions.Core.Models
{
    public class UserParameterItem : Interfaces.IUserParameterItem
    {
        public string UserId { get; set; }
        public string AssetPair { get; set; }
        public int TimeToFirstOption { get; set; }
        public int OptionLen { get; set; }
        public double PriceSize { get; set; }
        public int NPriceIndex { get; set; } 
        public int NTimeIndex { get; set; }
        public string ServerTimestamp { get; set; }
    }
}
