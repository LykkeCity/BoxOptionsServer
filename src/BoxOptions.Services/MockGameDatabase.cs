using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class MockGameDatabase : IGameDatabase
    {
        public Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IGameBetItem>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IUserHistoryItem>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IUserItem> LoadUserState(string userId)
        {
            throw new NotImplementedException();
        }

        public Task SaveGameBet(IGameBetItem bet)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserHistory(IUserHistoryItem history)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserState(IUserItem userState)
        {
            throw new NotImplementedException();
        }
    }
}
