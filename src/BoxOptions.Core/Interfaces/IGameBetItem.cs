using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IGameBetItem
    {
        string UserId { get; }
        string BoxId { get; }
        string AssetPair { get; }
        DateTime Date { get; }
        string Box { get; }
        string BetAmount { get; }
        string Parameters { get; }
        int BetStatus { get; }
    }
}
