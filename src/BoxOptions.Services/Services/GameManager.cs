using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using Common.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using WampSharp.V2.Realm;

namespace BoxOptions.Services
{
    public class GameManager : IGameManager, IDisposable
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
            BetLost = 10,
            BalanceChanged = 11
        }
                
        static System.Threading.SemaphoreSlim coeffCalculatorSemaphoreSlim = new System.Threading.SemaphoreSlim(1, 1);
        static int MaxUserBuffer = 128;

        public event EventHandler<BoxEventArgs> BetWin;
        public event EventHandler<BoxEventArgs> BetLose;

        
        List<UserState> userList;

        private readonly IGameDatabase database;
        private readonly ICoefficientCalculator calculator;
        private readonly IAssetQuoteSubscriber quoteFeed;
        private readonly IMicrographCache micrographCache;
        private readonly ISubject<BetResult> subject;
        private readonly ILog log;
        private readonly BoxOptionsSettings settings;

        public GameManager(BoxOptionsSettings settings, IGameDatabase database, ICoefficientCalculator calculator, 
            IAssetQuoteSubscriber quoteFeed, IMicrographCache micrographCache, IWampHostedRealm realm, ILog log)
        {
            this.database = database;
            this.calculator = calculator;
            this.quoteFeed = quoteFeed;
            this.settings = settings;
            this.micrographCache = micrographCache;
            this.log = log;
            this.subject = realm.Services.GetSubject<BetResult>(this.settings.BoxOptionsApi.GameManager.GameTopicName);


            if (this.settings != null && this.settings.BoxOptionsApi != null && this.settings.BoxOptionsApi.GameManager != null)
                MaxUserBuffer = this.settings.BoxOptionsApi.GameManager.MaxUserBuffer;

            userList = new List<UserState>();

            quoteFeed.MessageReceived += QuoteFeed_MessageReceived;

            
        }
        public void Dispose()
        {
            quoteFeed.MessageReceived -= QuoteFeed_MessageReceived;            
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

                // keep list size to maxbuffer
                if (userList.Count >= MaxUserBuffer)
                {
                    var OlderUser = (from u in userList
                                     where u.CurrentGameId == null 
                                     orderby u.LastChange
                                     select u).FirstOrDefault();
                    if (OlderUser != null)
                        userList.Remove(OlderUser);
                    else
                        log.WriteWarningAsync("GameManager", "GetUserState", "", $"MaxUserBuffer overriden. More open games than [{MaxUserBuffer}]");
                }


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
                //retval.SetBalance(40.50m);
                // Save it to Database
                await database.SaveUserState(retval);

            }
            else
            {
                // Load User Parameters
                var userParameters = await database.LoadUserParameters(userId);
                retval.LoadParameters(userParameters);
                
                // Load Current game if CurrentGameId is filled
                if (!string.IsNullOrEmpty(retval.CurrentGameId))
                {
                    Game game = await database.LoadGame(retval.UserId, retval.CurrentGameId);
                    if (game != null)
                    {
                        // set game
                        retval.SetGame(game);

                        // load game bets                        
                        game.LoadBets(await database.LoadGameBets(userId, game.GameId));                        

                    }
                    else // game is not on database. Remove gameId
                    {
                        retval.CurrentGameId = null;
                        await database.SaveUserState(retval);
                    }
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

        private void SetUserStatus(UserState user, GameStatus status, string message = null)
        {
            var hist = user.SetStatus((int)status, message);
            // Save history to database
            database.SaveUserHistory(user.UserId, hist);
            // Save status to Database
            database.SaveUserState(user);
        }

        private void QuoteFeed_MessageReceived(object sender, Core.Models.InstrumentPrice e)
        {
            throw new NotImplementedException();
        }

        #region IGameManager

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
            database.SaveGame(userState.UserId, newgame);

            // Set Status, saves User to DB
            SetUserStatus(userState, GameStatus.GameStarted, $"Game Started[{assetPair}] Id={newgame.GameId}");            
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

            string logmessage = $"Game Closed[{userState.CurrentGame.AssetPair}] Id={userState.CurrentGame.GameId}";
            // Terminate game
            userState.CurrentGame.FinishDate = DateTime.UtcNow;
            database.SaveGame(userState.UserId, userState.CurrentGame);

            // Remove game from user
            userState.RemoveGame();

            // Set Status, saves User to DB
            SetUserStatus(userState, GameStatus.GameClosed, logmessage);
            
        }

        public void PlaceBet(string userId, string box, decimal bet)
        {
            Console.WriteLine("{0}> PlaceBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, bet);

            // Get user state
            UserState userState = GetUserState(userId);
                        
            //Get ongoing gameId
            if (userState.CurrentGame == null)
                throw new InvalidOperationException("User has no game ongoing.");

            // Validate balance
            if (bet > userState.Balance)
                throw new InvalidOperationException("User has no balance for the bet.");

            // TODO: Get Box from... somewhere            
            Box boxObject = Box.FromJson(box);
                        
            // Get Current Coeffs for Game's Assetpair
            CoeffParameters coef = userState.GetParameters(userState.CurrentGame.AssetPair);
            if (coef == null || 
                coef.PriceSize == 0 || 
                coef.OptionLen==0)
                throw new InvalidOperationException($"Coefficient parameters are not set for Asset Pair [{userState.CurrentGame.AssetPair}].");

            // Place Bet            
            GameBet newBet = userState.CurrentGame.PlaceBet(boxObject, bet, coef);
            newBet.TimeToGraphReached

            // TODO: Save bet to DB
            database.SaveGameBet(userState.UserId, userState.CurrentGame, newBet);

            // Set Status, saves User to DB            
            SetUserStatus(userState, GameStatus.BetPlaced, $"BetPlaced[{boxObject.Id}]. Bet={bet}");

        }
        
        public decimal SetUserBalance(string userId, decimal newBalance)
        {
            UserState userState = GetUserState(userId);
            userState.SetBalance(newBalance);
            
            // Save User to DB            
            database.SaveUserState(userState);

            // Log Balance Change
            SetUserStatus(userState, GameStatus.BalanceChanged, $"New Balance: {newBalance}");

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
            database.SaveUserParameters(userId, userState.UserCoeffParameters);
        }
        public CoeffParameters GetUserParameters(string userId, string pair)
        {
            UserState userState = GetUserState(userId);
            return userState.GetParameters(pair);
        }
        public string RequestUserCoeff(string userId, string pair)
        {
            UserState userState = GetUserState(userId);
            var parameters = userState.GetParameters(pair);
            Task<string> t = CoeffCalculatorRequest(userId, pair, parameters.TimeToFirstOption, parameters.OptionLen, parameters.PriceSize, parameters.NPriceIndex, parameters.NTimeIndex);
            t.Wait();
            return t.Result;
        }

        public void AddLog(string userId, string eventCode, string message)
        {
            UserState userState = GetUserState(userId);

            int ecode = -1;
            int.TryParse(eventCode, out ecode);
            SetUserStatus(userState, (GameStatus)ecode, message);
        }


        #endregion


        protected virtual void OnBetWin(BoxEventArgs e)
        {
            BetWin?.Invoke(this, e);
        }
        protected virtual void OnBetLose(BoxEventArgs e)
        {
            BetLose?.Invoke(this, e);
        }

        


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
