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
        public Task<IEnumerable<CoeffParameters>> LoadUserParameters(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserState> LoadUserState(string userId)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserParameters(string userId, IEnumerable<CoeffParameters> parameters)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserState(UserState userState)
        {
            throw new NotImplementedException();
        }
    }
}
