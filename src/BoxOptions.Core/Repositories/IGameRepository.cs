using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core.Repositories
{
    public interface IGameRepository
    {
        Task InsertGameBetAsync(IEnumerable<IGameBetItem> olapEntities);
        Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo);
        Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo, int betState);
    }
}
