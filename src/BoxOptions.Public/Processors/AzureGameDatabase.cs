using BoxOptions.Core;
using BoxOptions.Core.Models;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Common;
using System.Threading;

namespace BoxOptions.Public.Processors
{
    public class AzureGameDatabase : IGameDatabase, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;

        private readonly int _maxBuffer = 100;
        private readonly Timer _writeCheck;

        private List<UserState> _usersToSave;
        private List<UserHistory> _userHistoryToSave;
        private List<GameBet> _gameBetsToSave;

        private DateTime _lastUserDataWrite;
        private DateTime _lastUserHistoryWrite;
        private DateTime _lastGameBetWrite;

        static System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");

        public AzureGameDatabase(IUserRepository userRep, IGameRepository gameRep)
        {
            this._userRepository = userRep;
            this._gameRepository = gameRep;

            _usersToSave = new List<UserState>();
            _userHistoryToSave = new List<UserHistory>();
            _gameBetsToSave = new List<GameBet>();

            _writeCheck = new Timer(WriteCheckCallback, null, 60000, 10000);
        }
               
        public Task SaveUserState(UserState userState)
        {
            if (userState == null)
                return Task.FromResult(0);


            var existingUser = _usersToSave.Where(u => u.UserId == userState.UserId).FirstOrDefault();
            if (existingUser != null)
                _usersToSave.Remove(existingUser);

            _usersToSave.Add(userState);
            if (_usersToSave.Count > _maxBuffer)            
                InsertUserBatchAsync();

            return Task.FromResult(0);
        }
        public async Task<UserState> LoadUserState(string userId)
        {
            var res = await _userRepository.GetUser(userId);
            if (res == null)
                return null;

            decimal balance = decimal.Parse(string.IsNullOrEmpty(res.Balance) ? "0" : res.Balance, CI);
            UserState retval = new UserState(res.UserId, balance, res.CurrentState)
            {
                LastChange = res.LastChange
            };


            // TODO: load user parameters and history
            //retval.StatusHistory

            return retval;

        }

        public Task SaveUserHistory(UserHistory history)
        {
            if (history == null)
                return Task.FromResult(0);

            _userHistoryToSave.Add(history);
            if (_userHistoryToSave.Count > _maxBuffer)
            {
                InsertHistoryBatchAsync();
            }
            return Task.FromResult(0);
        }
        public async Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {
            var userHist = await _userRepository.GetUserHistory(userId, dateFrom, dateTo);

            var converted = from p in userHist
                            select new UserHistory(p.UserId)
                            {
                                Timestamp = p.Date,
                                Status = int.Parse(p.Status),
                                Message = p.Message,
                                AccountDelta = p.AccountDelta
                            };
            return converted;
        }

        public Task SaveGameBet(GameBet bet)
        {
            if (bet == null)
                return Task.FromResult(0);

            var existingBet = _gameBetsToSave.Where(b => b.Box.Id == bet.Box.Id).FirstOrDefault();
            if (existingBet != null)
                _gameBetsToSave.Remove(existingBet);

            _gameBetsToSave.Add(bet);
            if (_gameBetsToSave.Count > _maxBuffer)
                InsertGameBetBatchAsync();

            return Task.FromResult(0);
        }
        public async Task<IEnumerable<GameBet>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            //throw new NotImplementedException();
            var gameBets = await _gameRepository.GetGameBetsByUser(userId, dateFrom, dateTo, betState);

            var converted = from p in gameBets
                            select new GameBet(userId)
                            {
                                BetAmount = decimal.Parse(p.BetAmount, CI),
                                Box = Box.FromJson(p.Box),
                                Timestamp = p.Date,
                                CurrentParameters = p.Parameters.DeserializeJson<BoxSize>(),
                                AssetPair = p.AssetPair,
                                BetStatus = (Services.BetStates)p.BetStatus
                            };
            return converted;
        }

        public async Task<IEnumerable<string>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }
        public async Task<IEnumerable<GameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return await _gameRepository.GetGameBetsByUser(userId, dateFrom, dateTo);
        }
        
        private void InsertUserBatchAsync()
        {
            UserState[] userBuffer;
            lock (this)
            {
                userBuffer = new UserState[_usersToSave.Count];
                _usersToSave.CopyTo(userBuffer);
                _usersToSave.Clear();
            }

            Thread t = new Thread(new ParameterizedThreadStart(InsertUserBatchTSig));
            t.Start(userBuffer);
        }
        private void InsertUserBatchTSig(object users)
        {
            UserState[] u = users as UserState[];
            InsertUserBatch(u);
        }
        private void InsertUserBatch(UserState[] users)
        {
            _lastUserDataWrite = DateTime.UtcNow;
            if (users == null || users.Length < 1)
                return;

            var usr = from u in users
                      select new UserItem()
                      {
                          Balance = u.Balance.ToString(CI),
                          CurrentState = u.CurrentState,
                          LastChange = u.LastChange,
                          UserId = u.UserId
                      };
            _userRepository.InsertUserAsync(usr);

        }

        private void InsertHistoryBatchAsync()
        {
            UserHistory[] hBuffer;
            lock (this)
            {
                hBuffer = new UserHistory[_userHistoryToSave.Count];
                _userHistoryToSave.CopyTo(hBuffer);
                _userHistoryToSave.Clear();
            }
            Thread t = new Thread(new ParameterizedThreadStart(InsertHistoryBatchTSig));
            t.Start(hBuffer);
        }
        private void InsertHistoryBatchTSig(object userHistory)
        {
            UserHistory[] h = userHistory as UserHistory[];
            InsertHistoryBatch(h);
        }
        private void InsertHistoryBatch(UserHistory[] history)
        {
            _lastUserHistoryWrite = DateTime.UtcNow;
            if (history == null || history.Length < 1)
                return;
                        
            var hst = from h in history
                      select new UserHistoryItem()
                      {
                          UserId = h.UserId,
                          Date = h.Timestamp,
                          Status = h.Status.ToString(),
                          Message = h.Message,
                          AccountDelta = h.AccountDelta
                      };

            _userRepository.InsertHistoryAsync(hst);
        }

        private void InsertGameBetBatchAsync()
        {
            GameBet[] bBuffer;
            lock (this)
            {
                bBuffer = new GameBet[_gameBetsToSave.Count];
                _gameBetsToSave.CopyTo(bBuffer);
                _gameBetsToSave.Clear();
            }
            Thread t = new Thread(new ParameterizedThreadStart(InsertGameBetBatchTSig));
            t.Start(bBuffer);
        }
        private void InsertGameBetBatchTSig(object bets)
        {
            GameBet[] b = bets as GameBet[];
            InsertGameBetBatch(b);
        }
        private void InsertGameBetBatch(GameBet[] bets)
        {
            _lastGameBetWrite = DateTime.UtcNow;
            if (bets == null || bets.Length < 1)
                return;
            
            var bts = from b in bets
                      select new GameBetItem()
                      {
                          UserId = b.UserId,
                          BetAmount = b.BetAmount.ToString(CI),
                          Box = b.Box.ToJson(),
                          Date = b.Timestamp,
                          Parameters = b.CurrentParameters.ToJson(),
                          AssetPair = b.AssetPair,
                          BetStatus = (int)b.BetStatus,
                          BoxId = b.Box.Id
                      };

            _gameRepository.InsertGameBetAsync(bts);
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
            if (_usersToSave.Count > 0)
                InsertUserBatchAsync();

            if (_gameBetsToSave.Count > 0)            
                InsertGameBetBatchAsync();

            if (_userHistoryToSave.Count > 0)            
                InsertHistoryBatchAsync();
        }
    }
}
