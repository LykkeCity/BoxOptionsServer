using BoxOptions.Common.Interfaces;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class GameManager : IGameManager
    {
        public enum GameStatus
        {
            Created = 0,
            Launch = 1,
            Wake = 2,
            Sleep = 3,
            GameStarted = 4,
            GameClosed = 5,
            ChangeBet = 6,
            ChangeScale = 7,
            BetPlaced = 8,
            BetWon = 9,
            BetLost = 10
        }
                
        static System.Threading.SemaphoreSlim coeffCalculatorSemaphoreSlim = new System.Threading.SemaphoreSlim(1, 1);

        public event EventHandler<BoxEventArgs> BetWin;
        public event EventHandler<BoxEventArgs> BetLose;

        

        List<UserState> userList;
        IGameDatabase database;
        ICoefficientCalculator calculator;
        IMicrographCache graphCache;

        public GameManager(IGameDatabase database, ICoefficientCalculator calculator, IMicrographCache graphCache)
        {
            this.database = database;
            this.calculator = calculator;
            this.graphCache = graphCache;
            userList = new List<UserState>();
        }


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

                // add it to cache
                userList.Add(current);
            }

            return current;

        }

        private async Task<UserState> LoadUserStateFromDb(string userId)
        {
            //await MutexTestAsync();
            //MutexTest();
            //Console.WriteLine("MutexTestAsync Done");

            // Database object fetch
            UserState retval = await database.LoadUserState(userId);

            if (retval == null)
            {
                // UserState not in database
                // Create new
                retval = new UserState(userId);

                // Save it to Database
                await database.SaveUserState(retval);
            }
            else
            {
                // Load Current game if CurrentGameId is filled
                if (!string.IsNullOrEmpty(retval.CurrentGameId))
                {
                    Game game = await database.LoadGame(retval.CurrentGameId);
                    retval.SetGame(game);
                }
            }

            return retval;
        }

        

        private async Task<string> CoeffCalculatorRequest(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            Guid fid = Guid.NewGuid();
            Console.WriteLine("{0} | {1}> CoeffCalculatorRequest for {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid, pair);

            await coeffCalculatorSemaphoreSlim.WaitAsync();
            Console.WriteLine("{0} | {1}> Aquired Semaphore", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
            try
            {
                Console.WriteLine("{0} | {1}> ChangeAsync... ", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
                string res = await calculator.ChangeAsync(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
                Console.WriteLine("{0} | {1}> ChangeAsync Finished. ", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);

                if (res != "OK")
                    throw new InvalidOperationException(res);

                Console.WriteLine("{0} | {1}> RequestAsync... ", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
                string result = await calculator.RequestAsync(userId, pair);
                Console.WriteLine("{0} | {1}> RequestAsync Finished... ", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
                return result;

            }
            finally
            {
                Console.WriteLine("{0} | {1}> Semaphore Released", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
                coeffCalculatorSemaphoreSlim.Release();
            }

        }

        //public void SetUserBalance(string userId, decimal newBalance)
        //{
        //    UserState userState = GetUserState(userId);
        //    userState.SetBalance(newBalance);
        //}
        

        #region IGameManager
        public void Launch(string userId)
        {
            Console.WriteLine("{0}> Launch({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);

            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Launch);            
        }
        
        public void Sleep(string userId)
        {
            Console.WriteLine("{0}> Sleep({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Sleep);            
        }        

        public void Wake(string userId)
        {
            Console.WriteLine("{0}> Wake({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Wake);
        }

        public string GameStart(string userId, string assetPair)
        {
            Console.WriteLine("{0}> GameStart({1} - {2})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair);

            // Get user state
            UserState userState = GetUserState(userId);

            // Game ongoing, must be closed first
            if (userState.CurrentGame != null)
                throw new InvalidOperationException($"this user already has an ongoing game [{userState.CurrentGame.AssetPair}]");

            // Create new game

            Game newgame = new Game(assetPair, Guid.NewGuid().ToString());

            // TODO: set game parameters

            // Get coefficients from calculator
            // calculator.ChangeAsync
            // 


            // Assign Game to user;
            userState.SetGame(newgame);

            // Save gave to database
            database.SaveGame(newgame);


            // Save userState to database
            database.SaveUserState(userState);

            // Set Status
            userState.SetStatus((int)GameStatus.GameStarted);
            return "OK";
        }

        public void GameClose(string userId)
        {
            Console.WriteLine("{0}> GameClose({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);

            // Get user state
            UserState userState = GetUserState(userId);

            //Get ongoing gameId
            if (userState.CurrentGame == null)
                throw new InvalidOperationException("User has no game ongoing.");

            // TODO: Terminate game

            // Remove game from user
            userState.RemoveGame();

            // Set status
            userState.SetStatus((int)GameStatus.GameClosed);
        }

        public void PlaceBet(string userId, string box, decimal bet)
        {
            Console.WriteLine("{0}> PlaceBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, bet);

            // Get user state
            UserState userState = GetUserState(userId);
            var graphdata = graphCache.GetGraphData();
            Console.WriteLine(graphdata.Count());

            //Get ongoing gameId
            if (userState.CurrentGame == null)
                throw new InvalidOperationException("User has no game ongoing.");

            // Validate balance
            if (bet > userState.Balance)
                throw new InvalidOperationException("User has no balance for the bet.");

            // TODO: Get Box from... somewhere
            Box boxObject = new Box(); // TODO: Get the Box object from received string

            // TODO: Place Bet
            userState.CurrentGame.PlaceBet(boxObject, bet);

            // Set status
            userState.SetStatus((int)GameStatus.BetPlaced);
        }

        public void ChangeBet(string userId, string box, decimal betAmount)
        {
            Console.WriteLine("{0}> ChangeBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount);
            
        }

        public void ChangeScale(string userId, decimal scale)
        {
            Console.WriteLine("{0}> Wake({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);            
        }

        public decimal SetUserBalance(string userId, decimal newBalance)
        {
            UserState userState = GetUserState(userId);
            userState.SetBalance(newBalance);
            return newBalance;
        }
                
        public decimal GetUserBalance(string userId)
        {
            UserState userState = GetUserState(userId);
            return userState.Balance;
        }

        public void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            UserState userState = GetUserState(userId);
            userState.SetParameters(pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
        }

        public string RequestUserCoeff(string userId, string pair)
        {
            UserState userState = GetUserState(userId);
            var parameters = userState.GetParameters(pair);
            Task<string> t = CoeffCalculatorRequest(userId, pair, parameters.TimeToFirstOption, parameters.OptionLen, parameters.PriceSize, parameters.NPriceIndex, parameters.NTimeIndex);
            t.Wait();
            return t.Result;
        }
        #endregion




        //private void MutexTest()
        //{
        //    var gdata = graphCache.GetGraphData();

        //    int timeToFirstOption = 30000;
        //    int optionLen = 30000;
        //    double priceSize = 0.0002;
        //    int nPriceindex = 15;
        //    int nTimeIndex = 15;

        //    foreach (var item in gdata)
        //    {
        //        Task t = CoeffCalculatorRequest("USERID", item.Key, timeToFirstOption, optionLen, priceSize, nPriceindex, nTimeIndex);
        //        //t.Start();

        //        optionLen += 1000;
        //        priceSize += 0.0002;
        //    }
        //    Console.WriteLine("ss");
        //}
        //private async Task MutexTestAsync()
        //{
        //    var gdata = graphCache.GetGraphData();

        //    int timeToFirstOption = 30000;
        //    int optionLen = 30000;
        //    double priceSize = 0.0002;
        //    int nPriceindex = 15;
        //    int nTimeIndex = 15;

        //    foreach (var item in gdata)
        //    {
        //        if (item.Key == "BTCUSD")
        //            continue;
        //        string res = await CoeffCalculatorRequest("USERID", item.Key, timeToFirstOption, optionLen, priceSize, nPriceindex, nTimeIndex);
        //        Console.WriteLine(res);
        //        //t.Start();

        //        optionLen += 1000;
        //        priceSize += 0.0002;
        //    }
        //    Console.WriteLine("ss");
        //}


    }
}
