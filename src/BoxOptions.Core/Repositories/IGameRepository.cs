using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IGameRepository
    {
        Task InsertGameAsync(Interfaces.IGameItem olapEntity);
        Task<Models.GameItem> GetGame(string userId,string gameId);
        Task<IEnumerable<Models.GameItem>> GetGamesByUser(string userId, DateTime dateFrom, DateTime dateTo);

        Task InsertGameBetAsync(Interfaces.IGameBetItem olapEntity);        
        Task<IEnumerable<Models.GameBetItem>> GetGameBetsByUser(string userId, string gameId);
    }
}
