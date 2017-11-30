using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface ILogRepository
    {
        Task InsertAsync(ILogItem logItem);
        Task<IEnumerable<ILogItem>> GetRange(DateTime dateFrom, DateTime dateTo, string clientId);
        Task<IEnumerable<string>> GetClients();
        Task<IEnumerable<ILogItem>> GetAll(DateTime dateFrom, DateTime dateTo);
    }
}
