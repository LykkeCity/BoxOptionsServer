using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Core.Interfaces
{
    public interface IUserHistoryItem
    {
        string UserId { get; }
        int Status { get; }
        DateTime Date { get; }
        string Message { get; }
    }

}
