using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IUserRepository
    {
        Task InsertUserAsync(IEnumerable<Interfaces.IUserItem> olapEntity);
        Task<Models.UserItem> GetUser(string userId);
               
        
        Task InsertHistoryAsync(IEnumerable<Interfaces.IUserHistoryItem> olapEntity);
        Task<IEnumerable<Models.UserHistoryItem>> GetUserHistory(string userId, DateTime dateFrom, DateTime dateTo);

        Task<IEnumerable<string>> GetUsers();
    }
}
