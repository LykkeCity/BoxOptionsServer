using BoxOptions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using BoxOptions.Services.Models;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class MockGameDatabase : IGameDatabase
    {
        public Task<IEnumerable<GameBet>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo,int status)
        {
            throw new NotImplementedException();
        }

       
        public Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

      

        public Task<UserState> LoadUserState(string userId)
        {
            throw new NotImplementedException();
        }

        public Task SaveGameBet(GameBet bet)
        {
            return Task.FromResult(0);
        }

        public Task SaveUserHistory(UserHistory history)
        {
            return Task.FromResult(0);
        }

    

        public Task SaveUserState(UserState userState)
        {
            throw new NotImplementedException();
        }
    }
}
