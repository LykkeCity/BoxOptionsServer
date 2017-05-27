using BoxOptions.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameDatabase
    {
        Task SaveUserState(UserState userState);
        Task<UserState> LoadUserState(string userId);

        Task SaveGame(Game game);
        Task<Game> LoadGame(string gameId);

        Task SaveUserParameters(string userId, IEnumerable<CoeffParameters> parameters);
        Task<IEnumerable<CoeffParameters>> LoadUserParameters(string userId);
    }
}
