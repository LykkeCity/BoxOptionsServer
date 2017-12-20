using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IUserHistoryItem
    {
        string UserId { get; }
        int GameStatus { get; }
        DateTime Date { get; }
        string Message { get; }
        double AccountDelta { get; }
    }

}
