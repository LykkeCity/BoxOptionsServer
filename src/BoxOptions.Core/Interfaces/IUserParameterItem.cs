namespace BoxOptions.Core.Interfaces
{
    public interface IUserParameterItem
    {
        string UserId { get; }
        string AssetPair { get; }
        int TimeToFirstOption { get; }
        int OptionLen { get; }
        double PriceSize { get; }
        int NPriceIndex { get; }
        int NTimeIndex { get; }
    }
}
