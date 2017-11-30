using BoxOptions.Common.Models;
using System.Collections.Generic;

namespace BoxOptions.Common.Interfaces
{
    public interface IHistoryHolder
    {
        LinkedList<Price> GetHistory(string asset);
    }
}
