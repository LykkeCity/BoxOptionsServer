using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameDatabase
    {
        Task SaveUserState(UserState userState);
        Task<UserState> LoadUserState(string userId);
                
        Task SaveGameBet(GameBet bet);
        Task<IEnumerable<GameBet>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState);
        
        Task SaveUserHistory( UserHistory history);
        Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo);
    }
}
