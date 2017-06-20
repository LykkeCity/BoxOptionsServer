using System.Collections.Generic;
using BoxOptions.Core.Models;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IHistoryHolder
    {
        LinkedList<Price> GetHistory(string asset);
    }
}