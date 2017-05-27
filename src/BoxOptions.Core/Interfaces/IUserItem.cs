using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IUserItem
    {
        string UserId { get; }
        string Balance { get; }
        int CurrentState { get; }
        string CurrentGameId { get; }
        DateTime LastChange { get; }
    }
}
