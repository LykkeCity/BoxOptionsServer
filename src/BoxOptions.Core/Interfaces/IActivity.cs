namespace BoxOptions.Core.Interfaces
{
    public interface IActivity
    {
        string Id { get; }
        string Name { get; }
        string Instrument { get; }
        double[] ActivityArray { get; }
        bool IsDefault { get; }
    }
}
