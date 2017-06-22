using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{

    public interface ILogRepository
    {
        Task InsertAsync(Interfaces.ILogItem olapEntity);
        Task<IEnumerable<Models.LogItem>> GetRange(DateTime dateFrom, DateTime dateTo, string clientId);
        Task<IEnumerable<string>> GetClients();
        Task<IEnumerable<Models.LogItem>> GetAll(DateTime dateFrom, DateTime dateTo);
    }
}
