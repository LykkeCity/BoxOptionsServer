using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IGameItem
    {
        string UserId { get; }
        string GameId { get; }
        string AssetPair { get; }
        DateTime CreationDate { get; }
        DateTime FinishDate { get; }
    }
}
