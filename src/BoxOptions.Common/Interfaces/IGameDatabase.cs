using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IGameDatabase
    {
        Task SaveUserState(IUserItem userState);
        Task<IUserItem> LoadUserState(string userId);

        Task SaveGameBet(IGameBetItem bet);
        Task<IEnumerable<IGameBetItem>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState);

        Task SaveUserHistory(IUserHistoryItem history);
        Task<IEnumerable<IUserHistoryItem>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo);

        Task<IEnumerable<string>> GetUsers();
        Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo);
    }
}
