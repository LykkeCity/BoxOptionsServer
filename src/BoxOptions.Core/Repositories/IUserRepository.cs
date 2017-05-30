using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IUserRepository
    {
        Task InsertUserAsync(Interfaces.IUserItem olapEntity);
        Task<Models.UserItem> GetUser(string userId);
                
        Task InsertManyParametersAsync(IEnumerable<Interfaces.IUserParameterItem> olapEntities);
        Task<IEnumerable<Models.UserParameterItem>> GetUserParameters(string userId);

        Task InsertHistoryAsync(Interfaces.IUserHistoryItem olapEntity);
        Task<IEnumerable<Models.UserHistoryItem>> GetUserHistory(string userId, int numEntries);

    }
}
