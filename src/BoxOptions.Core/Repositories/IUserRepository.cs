using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IUserRepository
    {
        Task InsertUserAsync(IEnumerable<IUserItem> olapEntity);
        Task<IUserItem> GetUser(string userId);
        
        Task InsertHistoryAsync(IEnumerable<IUserHistoryItem> olapEntity);
        Task<IEnumerable<IUserHistoryItem>> GetUserHistory(string userId, DateTime dateFrom, DateTime dateTo);

        Task<IEnumerable<string>> GetUsers();
    }
}
