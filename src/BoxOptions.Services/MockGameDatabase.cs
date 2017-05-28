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
        public Task<Game> LoadGame(string gameId)
        {
            return Task.FromResult<Game>(null);
        }

        public Task<Game> LoadGame(string userId, string gameId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GameBet>> LoadGameBets(string userId, string gameId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, int numEntries)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CoeffParameters>> LoadUserParameters(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserState> LoadUserState(string userId)
        {
            return Task.FromResult<UserState>(null);
        }

        public Task SaveGame(Game game)
        {
            return Task.FromResult(0);
        }

        public Task SaveGame(string userId, Game game)
        {
            throw new NotImplementedException();
        }
              
        public Task SaveGameBet(string userId, Game game, GameBet bet)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserHistory(string userId, UserHistory history)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserParameters(string userId, IEnumerable<CoeffParameters> parameters)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserState(UserState userState)
        {
            return Task.FromResult(0);
        }
    }
}
