using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BoxOptions.Public.Processors
{
    public class AzureGameDatabase : IGameDatabase, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;

        private readonly int _maxBuffer = 100;
        private readonly Timer _writeCheck;

        private readonly List<IUserItem> _userCache;
        private readonly List<IUserHistoryItem> _userHistoryCache;
        private readonly List<IGameBetItem> _gameBetCache;

        private DateTime _lastUserDataWrite;
        private DateTime _lastUserHistoryWrite;
        private DateTime _lastGameBetWrite;
        
        public AzureGameDatabase(IUserRepository userRep, IGameRepository gameRep)
        {
            _userRepository = userRep;
            _gameRepository = gameRep;

            _userCache = new List<IUserItem>();
            _userHistoryCache = new List<IUserHistoryItem>();
            _gameBetCache = new List<IGameBetItem>();

            _writeCheck = new Timer(WriteCheckCallback, null, 60000, 10000);
        }
               
        public Task SaveUserState(IUserItem userState)
        {
            if (userState == null)
                return Task.FromResult(0);


            var existingUser = _userCache.Where(u => u.UserId == userState.UserId).FirstOrDefault();
            if (existingUser != null)
                _userCache.Remove(existingUser);

            _userCache.Add(userState);
            if (_userCache.Count > _maxBuffer)            
                InsertUserBatchAsync();

            return Task.FromResult(0);
        }
        public async Task<IUserItem> LoadUserState(string userId)
        {
            return await _userRepository.GetUser(userId);
        }

        public Task SaveUserHistory(IUserHistoryItem history)
        {
            if (history == null)
                return Task.FromResult(0);

            _userHistoryCache.Add(history);
            if (_userHistoryCache.Count > _maxBuffer)
            {
                InsertHistoryBatchAsync();
            }
            return Task.FromResult(0);
        }
        public async Task<IEnumerable<IUserHistoryItem>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return await _userRepository.GetUserHistory(userId, dateFrom, dateTo);
        }

        public Task SaveGameBet(IGameBetItem bet)
        {
            if (bet == null)
                return Task.FromResult(0);

            var existingBet = _gameBetCache.Where(b => b.BoxId == bet.BoxId).FirstOrDefault();
            if (existingBet != null)
                _gameBetCache.Remove(existingBet);

            _gameBetCache.Add(bet);
            if (_gameBetCache.Count > _maxBuffer)
                InsertGameBetBatchAsync();

            return Task.FromResult(0);
        }
        public async Task<IEnumerable<IGameBetItem>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            return await _gameRepository.GetGameBetsByUser(userId, dateFrom, dateTo, betState);
        }

        public async Task<IEnumerable<string>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }
        public async Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return await _gameRepository.GetGameBetsByUser(userId, dateFrom, dateTo);
        }
        
        private void InsertUserBatchAsync()
        {
            IUserItem[] userBuffer;
            lock (this)
            {
                userBuffer = new IUserItem[_userCache.Count];
                _userCache.CopyTo(userBuffer);
                _userCache.Clear();
            }

            Thread t = new Thread(new ParameterizedThreadStart(InsertUserBatchTSig));
            t.Start(userBuffer);
        }
        private void InsertUserBatchTSig(object users)
        {
            IUserItem[] u = users as IUserItem[];
            InsertUserBatch(u);
        }
        private void InsertUserBatch(IUserItem[] users)
        {
            _lastUserDataWrite = DateTime.UtcNow;
            if (users == null || users.Length < 1)
                return;           
            _userRepository.InsertUserAsync(users);
        }

        private void InsertHistoryBatchAsync()
        {
            IUserHistoryItem[] hBuffer;
            lock (this)
            {
                hBuffer = new IUserHistoryItem[_userHistoryCache.Count];
                _userHistoryCache.CopyTo(hBuffer);
                _userHistoryCache.Clear();
            }
            Thread t = new Thread(new ParameterizedThreadStart(InsertHistoryBatchTSig));
            t.Start(hBuffer);
        }
        private void InsertHistoryBatchTSig(object userHistory)
        {
            IUserHistoryItem[] h = userHistory as IUserHistoryItem[];
            InsertHistoryBatch(h);
        }
        private void InsertHistoryBatch(IUserHistoryItem[] history)
        {
            _lastUserHistoryWrite = DateTime.UtcNow;
            if (history == null || history.Length < 1)
                return;           
            _userRepository.InsertHistoryAsync(history);
        }

        private void InsertGameBetBatchAsync()
        {
            IGameBetItem[] bBuffer;
            lock (this)
            {
                bBuffer = new IGameBetItem[_gameBetCache.Count];
                _gameBetCache.CopyTo(bBuffer);
                _gameBetCache.Clear();
            }
            Thread t = new Thread(new ParameterizedThreadStart(InsertGameBetBatchTSig));
            t.Start(bBuffer);
        }
        private void InsertGameBetBatchTSig(object bets)
        {
            IGameBetItem[] b = bets as IGameBetItem[];
            InsertGameBetBatch(b);
        }
        private void InsertGameBetBatch(IGameBetItem[] bets)
        {
            _lastGameBetWrite = DateTime.UtcNow;
            if (bets == null || bets.Length < 1)
                return;
            _gameRepository.InsertGameBetAsync(bets);
        }

        private void WriteCheckCallback(object status)
        {
            if (DateTime.UtcNow > _lastUserDataWrite.AddSeconds(20))
                InsertUserBatchAsync();

            if (DateTime.UtcNow > _lastUserHistoryWrite.AddSeconds(20))
                InsertUserBatchAsync();

            if (DateTime.UtcNow > _lastGameBetWrite.AddSeconds(20))
                InsertGameBetBatchAsync();
        }

        public void Dispose()
        {
            if (_userCache.Count > 0)
                InsertUserBatchAsync();

            if (_gameBetCache.Count > 0)            
                InsertGameBetBatchAsync();

            if (_userHistoryCache.Count > 0)            
                InsertHistoryBatchAsync();
        }
    }
}
