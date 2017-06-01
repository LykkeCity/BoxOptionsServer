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
    public class AzureGameDatabase : IGameDatabase
    {
        IUserRepository userRep;
        IGameRepository gameRep;
        static System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");
        public AzureGameDatabase(IUserRepository userRep, IGameRepository gameRep)
        {
            this.userRep = userRep;
            this.gameRep = gameRep;
        }

                
        public Task SaveUserState(UserState userState)
        {
            if (userState == null)
                throw new ArgumentNullException();

            UserItem user = new UserItem()
            {
                UserId = userState.UserId,
                Balance = userState.Balance.ToString(CI),
                CurrentState = userState.CurrentState,
                LastChange = userState.LastChange
            };

            return userRep.InsertUserAsync(user);
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
        
        public Task SaveUserParameters(string userId, IEnumerable<CoeffParameters> parameters)
        {
            if (parameters == null )
                throw new ArgumentNullException();

            List<UserParameterItem> parlist = new List<UserParameterItem>();
            foreach (CoeffParameters par in parameters)
            {
                UserParameterItem userPar = new UserParameterItem()
                {
                    UserId = userId,
                    AssetPair = par.AssetPair,
                    TimeToFirstOption = par.TimeToFirstOption,
                    OptionLen = par.OptionLen,
                    PriceSize = par.PriceSize,
                    NPriceIndex = par.NPriceIndex,
                    NTimeIndex = par.NTimeIndex
                };
                parlist.Add(userPar);
            }

           

            return userRep.InsertManyParametersAsync(parlist);
        }
        public async Task<IEnumerable<CoeffParameters>> LoadUserParameters(string userId)
        {

            var userPars = await userRep.GetUserParameters(userId);

            var converted = from p in userPars
                            select new CoeffParameters()
                            {
                                AssetPair = p.AssetPair,
                                NPriceIndex = p.NPriceIndex,
                                NTimeIndex = p.NTimeIndex,
                                OptionLen = p.OptionLen,
                                PriceSize = p.PriceSize,
                                TimeToFirstOption = p.TimeToFirstOption
                            };
            return converted;
        }


        public Task SaveUserHistory(string userId, UserHistory history)
        {
            if (string.IsNullOrEmpty(userId) || history == null)
                throw new ArgumentNullException();

            UserHistoryItem hitem = new UserHistoryItem()
            {
                UserId = userId,
                Date = history.Timestamp,
                Status = history.Status.ToString(),
                Message = history.Message
            };

            return userRep.InsertHistoryAsync(hitem);
        }
        public async Task<IEnumerable<UserHistory>> LoadUserHistory(string userId, int numEntries)
        {
            var userHist = await userRep.GetUserHistory(userId, numEntries);

            var converted = from p in userHist
                            select new UserHistory()
                            {
                                Timestamp = p.Date,
                                Status = int.Parse(p.Status),
                                Message = p.Message
                            };
            return converted;
        }
                
        public Task SaveGameBet(string userId, GameBet bet)
        {
            
            UserParameterItem betpars = new UserParameterItem()
            {
                AssetPair = bet.CurrentParameters.AssetPair,
                NPriceIndex = bet.CurrentParameters.NPriceIndex,
                NTimeIndex = bet.CurrentParameters.NTimeIndex,
                OptionLen = bet.CurrentParameters.OptionLen,
                PriceSize = bet.CurrentParameters.PriceSize,
                TimeToFirstOption = bet.CurrentParameters.TimeToFirstOption,
                UserId = userId
            };

            GameBetItem newbet = new GameBetItem()
            {
                UserId = userId,                
                BetAmount = bet.BetAmount.ToString(CI),
                Box = bet.Box.ToJson(),
                Date = bet.Timestamp,
                Parameters = betpars.ToJson(),
                AssetPair = bet.AssetPair,
                BetStatus = (int)bet.BetStatus,
                BoxId = bet.Box.Id

            };
            return gameRep.InsertGameBetAsync(newbet);
        }

        public async Task<IEnumerable<GameBet>> LoadGameBets(string userId, int betState)
        {
            throw new NotImplementedException();
        //    var gameBets = await gameRep.GetGameBetsByUser(userId, betState);

        //    var converted = from p in gameBets
        //                    select new GameBet(userId)
        //                    {
        //                        BetAmount = decimal.Parse(p.BetAmount, CI),
        //                        Box = Box.FromJson(p.Box),
        //                        Timestamp = p.Date,
        //                        CurrentParameters = CoeffParameters.FromJson(p.Parameters),
        //                        AssetPair = p.AssetPair,
        //                        BetStatus = (GameBet.BetStates)p.BetStatus                                
        //                    };
        //    return converted;
        }
    }
}
