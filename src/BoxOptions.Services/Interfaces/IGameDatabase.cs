using BoxOptions.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameDatabase
    {
        Task SaveUserState(UserState userState);
        Task<UserState> LoadUserState(string userId);

        Task SaveGame(string userId, Game game);
        Task<Game> LoadGame(string userId, string gameId);

        Task SaveGameBet(string userId, Game game, GameBet bet);
        Task<IEnumerable<GameBet>> LoadGameBets(string userId, string gameId);

        Task SaveUserParameters(string userId, IEnumerable<CoeffParameters> parameters);
        Task<IEnumerable<CoeffParameters>> LoadUserParameters(string userId);

        Task SaveUserHistory(string userId, UserHistory history);
        Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, int numEntries);
    }
}
