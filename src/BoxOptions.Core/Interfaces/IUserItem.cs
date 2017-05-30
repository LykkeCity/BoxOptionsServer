using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IUserItem
    {
        string UserId { get; }
        string Balance { get; }
        int CurrentState { get; }
        DateTime LastChange { get; }
    }
}
