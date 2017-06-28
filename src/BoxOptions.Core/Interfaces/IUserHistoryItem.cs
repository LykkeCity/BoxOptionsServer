using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Core.Interfaces
{
    public interface IUserHistoryItem
    {
        string UserId { get; }
        string Status { get; }
        DateTime Date { get; }
        string Message { get; }
        double AccountDelta { get; }
    }

}
