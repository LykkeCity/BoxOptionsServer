namespace BoxOptions.Core.Interfaces
{
    public interface IActivity
    {
        string Name { get; }
        string Instrument { get; }
        decimal[] ActivityArray { get; }
    }
}
