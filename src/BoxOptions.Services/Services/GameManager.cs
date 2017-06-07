using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WampSharp.V2.Realm;


namespace BoxOptions.Services
{
    public class GameManager : IGameManager, IDisposable
    {
        #region Vars
        /// <summary>
        /// Coefficient Calculator Request Semaphore
        /// Mutual Exclusion Process
        /// </summary>
        static System.Threading.SemaphoreSlim coeffCalculatorSemaphoreSlim = new System.Threading.SemaphoreSlim(1, 1);
      
        static int MaxUserBuffer = 128;
                
        /// <summary>
        /// Users Cache
        /// </summary>
        List<UserState> userList;
                
        /// <summary>
        /// Database Object
        /// </summary>
        private readonly IGameDatabase database;
        /// <summary>
        /// CoefficientCalculator Object
        /// </summary>
        private readonly ICoefficientCalculator calculator;
                                
        /// <summary>
        /// Settings
        /// </summary>
        private readonly BoxOptionsSettings settings;
        
        #endregion

        #region Constructor
        public GameManager(BoxOptionsSettings settings, IGameDatabase database, 
            ICoefficientCalculator calculator)
        {
            this.database = database;
            this.calculator = calculator;            
            this.settings = settings;

            if (this.settings != null && this.settings.BoxOptionsApi != null && this.settings.BoxOptionsApi.GameManager != null)
                MaxUserBuffer = this.settings.BoxOptionsApi.GameManager.MaxUserBuffer;
                        
            userList = new List<UserState>();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Finds user object in User cache or loads it from DB if not in cache
        /// Opens Wamp Topic for User Client
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>User Object</returns>
        private UserState GetUserState(string userId)
        {
            var ulist = from u in userList
                        where u.UserId == userId
                        select u;
            if (ulist.Count() > 1)
                throw new InvalidOperationException("User State List has duplicate entries");

            UserState current = ulist.FirstOrDefault();
            if (current == null)
            {
                // UserState not in current cache,
                // download it from database
                Task<UserState> t = LoadUserStateFromDb(userId);                               
                t.Wait();
                current = t.Result;
                
                // keep list size to maxbuffer
                if (userList.Count >= MaxUserBuffer)
                {
                    var OlderUser = (from u in userList
                                     orderby u.LastChange
                                     select u).FirstOrDefault();

                    if (OlderUser != null)
                    {
                        // Remove user from cache
                        userList.Remove(OlderUser);
                    }
                }
                // add it to cache
                userList.Add(current);
            }
            return current;
        }

        /// <summary>
        /// Loads user object from DB
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>User Object</returns>
        private async Task<UserState> LoadUserStateFromDb(string userId)
        {            
            // Database object fetch
            UserState retval = await database.LoadUserState(userId);            

            if (retval == null)
            {
                // UserState not in database
                // Create new
                retval = new UserState(userId);                
                //retval.SetBalance(40.50m);
                // Save it to Database
                await database.SaveUserState(retval);

            }
            else
            {                
                // Load User Parameters
                var userParameters = await database.LoadUserParameters(userId);
                retval.LoadParameters(userParameters);
            }

            return retval;
        }
                
        //Dictionary<string, string> coeffChangecache = new Dictionary<string, string>();

        /// <summary>
        /// Performs a Coefficient Request to CoeffCalculator object
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="pair">Instrument</param>
        /// <param name="timeToFirstOption">Time to first option</param>
        /// <param name="optionLen">Option Length</param>
        /// <param name="priceSize">Price Size</param>
        /// <param name="nPriceIndex">NPrice Index</param>
        /// <param name="nTimeIndex">NTime Index</param>
        /// <returns>CoeffCalc result</returns>
        private async Task<string> CoeffCalculatorRequest(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            //Activate Mutual Exclusion Semaphor
            await coeffCalculatorSemaphoreSlim.WaitAsync();
            
            // Validate if there are changes in parameters asset parameters
            // If parameters changed since last CoeffAPI call a change command mus be sent
            // If parameters are the same, only a request is needed.
            bool hasChanges;
            
            /*
             * 
            string coeffs = string.Format($"{pair}_{timeToFirstOption}_{optionLen}_{priceSize}_{nPriceIndex}_{nTimeIndex}");
            if (!coeffChangecache.ContainsKey(pair))
            {
                coeffChangecache.Add(pair, coeffs);
                hasChanges = true;
            }
            else
            {
                string lastAssetCall = coeffChangecache[pair];

                if (lastAssetCall.Equals(coeffs, StringComparison.Ordinal))                                    
                    hasChanges = false;     // No change in coefficients for Asset since last call
                else              
                {      
                    hasChanges = true;      // Coefficient Prameters changed for Asset since last call
                    coeffChangecache[pair] = coeffs;
                }
            }
            *
            */

            // Always do a change before a request
            // If there is other client using CoeffAPI this cache is compromised
            hasChanges = true;

                        
            try
            {                                
                if (hasChanges)
                {                    
                    // Change calculator parameters for current pair with User parameters
                    string res = await calculator.ChangeAsync(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);                    
                    if (res != "OK")
                        throw new InvalidOperationException(res);
                }

                // Request calculator coefficients                
                return await calculator.RequestAsync(userId, pair);
            }
            finally { coeffCalculatorSemaphoreSlim.Release(); }

        }
                    
        /// <summary>
        /// Disposes GameManager Resources
        /// </summary>
        public void Dispose()
        {         
            userList = null;
        }
        #endregion
        
        #region IGameManager
        
        public void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            UserState userState = GetUserState(userId);

            // Validate Parameters
            bool ValidateParameters = calculator.ValidateChange(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            if (ValidateParameters == false)
            {
                // Invalid Parameters, throw error
                throw new ArgumentException("Invalid Parameters");
            }

            // Set User Parameters for AssetPair
            userState.SetParameters(pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            // Save User Parameters to DB
            database.SaveUserParameters(userId, userState.UserCoeffParameters);            
        }
        
        public string RequestUserCoeff(string userId, string pair)
        {            
            UserState userState = GetUserState(userId);
            // Load User Parameters
            var parameters = userState.GetParameters(pair);



            Console.WriteLine("{0} | CoeffCalculatorRequest= {1} > {2}",DateTime.Now.ToString("HH:mm:ss.fff") ,pair, userId);
            // Request Coeffcalculator Data            
            Task<string> t = CoeffCalculatorRequest(userId, pair, parameters.TimeToFirstOption, parameters.OptionLen, parameters.PriceSize, parameters.NPriceIndex, parameters.NTimeIndex);
            t.Wait();
            string result = t.Result;

            // Validate CoefCalculator Result
            string ValidationError;
            bool IsOk = calculator.ValidateRequestResult(result, out ValidationError);
            
            // Take action on validation result.
            if (IsOk)
            {
                // return CoeffCalcResult
                Console.WriteLine("{0} | Finished= {1} > {2}", DateTime.Now.ToString("HH:mm:ss.fff"), pair, userId);
                return result;
            }
            else
            {
                // Throw Exception
                throw new ArgumentException(ValidationError);
            }
        }
        
        #endregion
    }
}
