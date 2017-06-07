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
        static System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");
        public AzureGameDatabase(IUserRepository userRep)
        {
            this.userRep = userRep;            
        }

                
        public Task SaveUserState(UserState userState)
        {
            if (userState == null)
                throw new ArgumentNullException();

            UserItem user = new UserItem()
            {
                UserId = userState.UserId,
                Balance = "0",
                CurrentState = 0,
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
            UserState retval = new UserState(res.UserId)
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

    }
}
