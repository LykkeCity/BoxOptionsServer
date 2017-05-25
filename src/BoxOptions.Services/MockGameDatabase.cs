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

        public Task<UserState> LoadUserState(string userId)
        {
            return Task.FromResult<UserState>(null);
        }

        public Task SaveGame(Game game)
        {
            return Task.FromResult(0);
        }

        public Task SaveUserState(UserState userState)
        {
            return Task.FromResult(0);
        }
    }
}
