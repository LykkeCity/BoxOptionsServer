using BoxOptions.Core;
using BoxOptions.Core.Models;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Common;

namespace BoxOptions.Public.Processors
{
    public class AzureGameDatabase : IGameDatabase, IDisposable
    {
        IUserRepository userRep;
        IGameRepository gameRep;

        int maxBuffer = 64;



        List<UserState> usersToSave;
        List<UserHistory> userHistoryToSave;
        List<GameBet> gameBetsToSave;

        static System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");

        public AzureGameDatabase(IUserRepository userRep, IGameRepository gameRep)
        {
            this.userRep = userRep;
            this.gameRep = gameRep;

            usersToSave = new List<UserState>();
            userHistoryToSave = new List<UserHistory>();
            gameBetsToSave = new List<GameBet>();
        }


        public Task SaveUserState(UserState userState)
        {
            if (userState == null)
                return Task.FromResult(0);


            var existingUser = usersToSave.Where(u => u.UserId == userState.UserId).FirstOrDefault();
            if (existingUser != null)
                usersToSave.Remove(existingUser);

            usersToSave.Add(userState);
            if (usersToSave.Count > maxBuffer)
            {
                UserState[] userBuffer = new UserState[usersToSave.Count];
                usersToSave.CopyTo(userBuffer);
                usersToSave.Clear();
                InsertUserBatchAsync(userBuffer);
            }
            return Task.FromResult(0);
        }
        public async Task<UserState> LoadUserState(string userId)
        {
            var res = await userRep.GetUser(userId);
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

            userHistoryToSave.Add(history);
            if (userHistoryToSave.Count > maxBuffer)
            {
                UserHistory[] hBuffer = new UserHistory[userHistoryToSave.Count];
                userHistoryToSave.CopyTo(hBuffer);
                userHistoryToSave.Clear();
                InsertHistoryBatchAsync(hBuffer);
            }
            return Task.FromResult(0);
        }
        public async Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {
            var userHist = await userRep.GetUserHistory(userId, dateFrom, dateTo);

            var converted = from p in userHist
                            select new UserHistory(p.UserId)
                            {
                                Timestamp = p.Date,
                                Status = int.Parse(p.Status),
                                Message = p.Message
                            };
            return converted;
        }

        public Task SaveGameBet(GameBet bet)
        {
            if (bet == null)
                return Task.FromResult(0);

            var existingBet = gameBetsToSave.Where(b => b.Box.Id == bet.Box.Id).FirstOrDefault();
            if (existingBet != null)
                gameBetsToSave.Remove(existingBet);

            gameBetsToSave.Add(bet);
            if (gameBetsToSave.Count > maxBuffer)
            {
                GameBet[] bBuffer = new GameBet[gameBetsToSave.Count];
                gameBetsToSave.CopyTo(bBuffer);
                gameBetsToSave.Clear();

                InsertGameBetBatchAsync(bBuffer);
            }

            return Task.FromResult(0);



        }
        public async Task<IEnumerable<GameBet>> LoadGameBets(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            //throw new NotImplementedException();
            var gameBets = await gameRep.GetGameBetsByUser(userId, dateFrom, dateTo, betState);

            var converted = from p in gameBets
                            select new GameBet(userId)
                            {
                                BetAmount = decimal.Parse(p.BetAmount, CI),
                                Box = Box.FromJson(p.Box),
                                Timestamp = p.Date,
                                CurrentParameters = p.Parameters.DeserializeJson<BoxSize>(),
                                AssetPair = p.AssetPair,
                                BetStatus = (GameBet.BetStates)p.BetStatus
                            };
            return converted;
        }

        
        private void InsertUserBatchAsync(UserState[] users)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InsertUserBatchTSig));
            t.Start(users);
        }
        private void InsertUserBatchTSig(object users)
        {
            UserState[] u = users as UserState[];
            InsertUserBatch(u);
        }
        private void InsertUserBatch(UserState[] users)
        {
            if (users == null || users.Length < 1)
                return;

            Console.WriteLine("{0} - DB.InsertUserBatch - Entries:[{1}]", DateTime.UtcNow.ToString("HH:mm:ss.fff"), users.Length);

            var usr = from u in users
                      select new UserItem()
                      {
                          Balance = u.Balance.ToString(CI),
                          CurrentState = u.CurrentState,
                          LastChange = u.LastChange,
                          UserId = u.UserId
                      };
            userRep.InsertUserAsync(usr);

        }

        private void InsertHistoryBatchAsync(UserHistory[] history)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InsertHistoryBatchTSig));
            t.Start(history);
        }
        private void InsertHistoryBatchTSig(object userHistory)
        {
            UserHistory[] h = userHistory as UserHistory[];
            InsertHistoryBatch(h);
        }
        private void InsertHistoryBatch(UserHistory[] history)
        {
            if (history == null || history.Length < 1)
                return;

            Console.WriteLine("{0} - DB.InsertHistoryBatch - Entries:[{1}]", DateTime.UtcNow.ToString("HH:mm:ss.fff"), history.Length);

            var hst = from h in history
                      select new UserHistoryItem()
                      {
                          UserId = h.UserId,
                          Date = h.Timestamp,
                          Status = h.Status.ToString(),
                          Message = h.Message
                      };

            userRep.InsertHistoryAsync(hst);
        }

        private void InsertGameBetBatchAsync(GameBet[] bets)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InsertGameBetBatchTSig));
            t.Start(bets);
        }
        private void InsertGameBetBatchTSig(object bets)
        {
            GameBet[] b = bets as GameBet[];
            InsertGameBetBatch(b);
        }
        private void InsertGameBetBatch(GameBet[] bets)
        {
            if (bets == null || bets.Length < 1)
                return;

            Console.WriteLine("{0} - DB.InsertGameBetBatch - Entries:[{1}]", DateTime.UtcNow.ToString("HH:mm:ss.fff"), bets.Length);

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

            gameRep.InsertGameBetAsync(bts);
        }

        public void Dispose()
        {
            if (usersToSave.Count > 0)
            {
                UserState[] userBuffer = new UserState[usersToSave.Count];
                usersToSave.CopyTo(userBuffer);
                usersToSave.Clear();
                InsertUserBatchAsync(userBuffer);
            }

            if (gameBetsToSave.Count > 0)
            {
                GameBet[] bBuffer = new GameBet[gameBetsToSave.Count];
                gameBetsToSave.CopyTo(bBuffer);
                gameBetsToSave.Clear();

                InsertGameBetBatchAsync(bBuffer);
            }

            if (userHistoryToSave.Count > 0)
            {
                UserHistory[] hBuffer = new UserHistory[userHistoryToSave.Count];
                userHistoryToSave.CopyTo(hBuffer);
                userHistoryToSave.Clear();
                InsertHistoryBatch(hBuffer);
            }

            
        }
    }
}
