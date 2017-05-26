﻿using BoxOptions.Services.Models;
using System.Threading.Tasks;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameDatabase
    {
        Task SaveUserState(UserState userState);
        Task<UserState> LoadUserState(string userId);

        Task SaveGame(Game game);
        Task<Game> LoadGame(string gameId);
    }
}