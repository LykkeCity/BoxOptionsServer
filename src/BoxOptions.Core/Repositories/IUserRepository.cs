using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IUserRepository
    {
        Task InsertUserAsync(Interfaces.IUserItem olapEntity);
        Task<Models.UserItem> GetUser(string userId);
                
        Task InsertManyParametersAsync(IEnumerable<Interfaces.IUserParameterItem> olapEntities);
        Task<IEnumerable<Models.UserParameterItem>> GetUserParameters(string userId);

        Task InsertManyHistoryAsync(IEnumerable<Interfaces.IUserHistoryItem> olapEntities);
        Task<IEnumerable<Models.UserHistoryItem>> GetUserHistory(string userId, int numEntries);

    }
}
