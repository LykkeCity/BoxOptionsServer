namespace BoxOptions.Core.Interfaces
{
    public interface ILogItem
    {
        string ClientId { get; }
        string EventCode { get; }
        string Message { get; }
    }
}
