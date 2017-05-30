using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core;
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
            BalanceChanged = 11,
            ParameterChanged = 12,
            CoeffRequest = 13
        }
                
        static System.Threading.SemaphoreSlim coeffCalculatorSemaphoreSlim = new System.Threading.SemaphoreSlim(1, 1);
        static System.Threading.SemaphoreSlim quoteReceivedSemaphoreSlim = new System.Threading.SemaphoreSlim(1, 1);
        static int MaxUserBuffer = 128;

        List<GameBet> betCache;
        List<UserState> userList;
        
        public event EventHandler<BetEventArgs> BetWin;
        public event EventHandler<BetEventArgs> BetLose;
        
        private readonly IGameDatabase database;
        private readonly ICoefficientCalculator calculator;
        private readonly IAssetQuoteSubscriber quoteFeed;        
        private readonly ISubject<BetResult> subject;
        private readonly ILogRepository logRepository;
        private readonly BoxOptionsSettings settings;

        private Dictionary<string, PriceCache> assetCache;

        public GameManager(BoxOptionsSettings settings, IGameDatabase database, ICoefficientCalculator calculator, 
            IAssetQuoteSubscriber quoteFeed, IWampHostedRealm realm, ILogRepository logRepository)
        {
            this.database = database;
            this.calculator = calculator;
            this.quoteFeed = quoteFeed;
            this.settings = settings;            
            this.logRepository = logRepository;
            this.subject = realm.Services.GetSubject<BetResult>(this.settings.BoxOptionsApi.GameManager.GameTopicName);


            if (this.settings != null && this.settings.BoxOptionsApi != null && this.settings.BoxOptionsApi.GameManager != null)
                MaxUserBuffer = this.settings.BoxOptionsApi.GameManager.MaxUserBuffer;

            userList = new List<UserState>();
            betCache = new List<GameBet>();
            assetCache = new Dictionary<string, PriceCache>();

            quoteFeed.MessageReceived += QuoteFeed_MessageReceived;

            
        }
        public void Dispose()
        {            
            quoteFeed.MessageReceived -= QuoteFeed_MessageReceived;
            betCache = null;

            foreach (var user in userList)
            {
                foreach (var bet in user.OpenBets)
                {
                    bet.Dispose();
                } 
            }

            userList = null;

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
                                     orderby u.LastChange
                                     select u).FirstOrDefault();
                    if (OlderUser != null)
                        userList.Remove(OlderUser);
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

                // TODO: Load User Bets                
                //var bets = await database.LoadGameBets(userId, (int)GameBet.BetStates.OnGoing);
                //retval.LoadBets(bets);



            }

            return retval;
        }
        
        private async Task<string> CoeffCalculatorRequest(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {            
            await coeffCalculatorSemaphoreSlim.WaitAsync();
            try
            {
                // Change calculator parameters for current pair with User parameters
                string res = await calculator.ChangeAsync(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
                if (res != "OK")
                    throw new InvalidOperationException(res);

                // Request calculator coefficients
                return await calculator.RequestAsync(userId, pair);
            }
            finally { coeffCalculatorSemaphoreSlim.Release(); }

        }

        private void SetUserStatus(string userId, GameStatus status, string message = null)
        {
            UserState user = GetUserState(userId);
            SetUserStatus(user, status, message);
        }
        private void SetUserStatus(UserState user, GameStatus status, string message = null)
        {
            var hist = user.SetStatus((int)status, message);
            // Save history to database
            database.SaveUserHistory(user.UserId, hist);
            // Save status to Database
            database.SaveUserState(user);

            logRepository.InsertAsync(new Core.Models.LogItem
            {
                ClientId = user.UserId,
                EventCode = ((int)status).ToString(),
                Message = message
            });
        }
                
        private bool CheckBet(GameBet sdr, decimal currentPrice, decimal previousPrice)
        {

            if ((currentPrice > sdr.Box.MinPrice && currentPrice < sdr.Box.MaxPrice) ||       // currentPrice> minPrice and currentPrice<maxPrice
                (previousPrice > sdr.Box.MaxPrice && currentPrice < sdr.Box.MinPrice) ||     // OR previousPrice > maxPrice and currentPrice < minPrice
                (previousPrice < sdr.Box.MinPrice && currentPrice > sdr.Box.MaxPrice))      // OR previousPrice < minPrice and currentPrice > maxPrice
                return true;
            else
                return false;
        }

        private void ProcessBetWin(GameBet bet)
        {            
            bet.BetStatus = GameBet.BetStates.Win;
            // Publish WIN to WAMP topic
            PublishWinAsync(bet);
            // Save to Database
            database.SaveGameBet(bet.UserId, bet);
        }
        private void ProcessBetTimeOut(GameBet bet)
        {
            // Remove bet from cache
            bool res = betCache.Remove(bet);
            //Console.WriteLine("{0}> CacheSize:[{1}]Bet removed from cache {2}.", DateTime.Now.ToString("HH:mm:ss.fff"), betCache.Count, bet.Box.Id);

            if (bet.BetStatus != GameBet.BetStates.Win)
            {
                // If bet was not won previously
                // publish LOSE to WAMP topic
                bet.BetStatus = GameBet.BetStates.Lose;

                PublishLoseAsync(bet);
                database.SaveGameBet(bet.UserId, bet);
            }
            //Console.WriteLine("{0}>ProcessBetTimeOut ={1}", DateTime.Now.ToString("HH:mm:ss.fff"), bet.Box.Id);
        }


        private void PublishWinAsync(GameBet bet)
        {
            var t = Task.Run(() => {
                BetResult res = new BetResult()
                {
                    BoxId = bet.Box.Id,
                    BetAmount = bet.BetAmount,
                    IsWin = true,
                    Timestamp = DateTime.UtcNow
                };
                // Publish to WAMP topic
                subject.OnNext(res);
                // Raise OnBetWin Event
                OnBetWin(new BetEventArgs(bet));

                SetUserStatus(bet.UserId, GameStatus.BetWon, $"Bet WON [{bet.Box.Id}] [{bet.AssetPair}] Bet:{bet.BetAmount}");
            });
        }
        private void PublishLoseAsync(GameBet bet)
        {
            var t = Task.Run(() => {
                BetResult res = new BetResult()
                {
                    BoxId = bet.Box.Id,
                    BetAmount = bet.BetAmount,
                    IsWin = false,
                    Timestamp = DateTime.UtcNow
                };
                // Publish to WAMP topic
                subject.OnNext(res);
                // Raise OnBetLose Event
                OnBetLose(new BetEventArgs(bet));

                SetUserStatus(bet.UserId, GameStatus.BetLost, $"Bet LOST [{bet.Box.Id}] [{bet.AssetPair}] Bet:{bet.BetAmount}");
            });
        }


        #region Event Handlers
        private void QuoteFeed_MessageReceived(object sender, Core.Models.InstrumentPrice e)
        {
            Guid fid = Guid.NewGuid();
            //Console.WriteLine("{0} | {1}> QuoteFeed Received [{2}]{3}/{4}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid, e.Instrument, e.Bid, e.Ask);
            quoteReceivedSemaphoreSlim.WaitAsync();
            //Console.WriteLine("{0} | {1}> Aquired Semaphore", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
            try
            {

                // Add price to cache
                if (!assetCache.ContainsKey(e.Instrument))
                    assetCache.Add(e.Instrument, new PriceCache());

                // Do not process if price ask did not change
                if (assetCache[e.Instrument].CurrentPrice == (decimal)e.Ask)
                    return;

                assetCache[e.Instrument].PreviousPrice = assetCache[e.Instrument].CurrentPrice;
                assetCache[e.Instrument].CurrentPrice = (decimal)e.Ask;

                //Console.WriteLine("{0} | {1}> >> PreviousPrice:[{2}] CurrentPrice:[{3}]", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid, assetCache[e.Instrument].PreviousPrice, assetCache[e.Instrument].CurrentPrice);

                // Get bets for current asset
                // That are not yet with WIN status
                var assetBets = from b in betCache
                                where b.AssetPair == e.Instrument &&
                                b.BetStatus != GameBet.BetStates.Win
                                select b;
                if (assetBets.Count() == 0)
                    return;


                foreach (var bet in assetBets)
                {
                    // Check open bets for
                    bool IsWin = CheckBet(bet, assetCache[e.Instrument].CurrentPrice, assetCache[e.Instrument].PreviousPrice);
                    
                    if (IsWin)
                        ProcessBetWin(bet);

                }
            }
            finally
            {
                //Console.WriteLine("{0} | {1}> Finished", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
                quoteReceivedSemaphoreSlim.Release();
                //Console.WriteLine("{0} | {1}> Released Semaphore", DateTime.UtcNow.ToString("HH:mm:ss.fff"), fid);
            }

        }

        private void Bet_TimeToGraphReached(object sender, EventArgs e)
        {
            GameBet sdr = sender as GameBet;
            if (sdr == null)
                return;

            //Console.WriteLine("{0}>Bet_TimeToGraphReached={1}", DateTime.Now.ToString("HH:mm:ss.fff"), sdr.Box.TimeToGraph);

            // Do initial Check
            bool IsWin = false;
            if (assetCache.ContainsKey(sdr.AssetPair))
            {
                if (assetCache[sdr.AssetPair].CurrentPrice > 0 && assetCache[sdr.AssetPair].PreviousPrice > 0)
                    IsWin = CheckBet(sdr, assetCache[sdr.AssetPair].CurrentPrice, assetCache[sdr.AssetPair].PreviousPrice);
            }
            
            // Set bet as with WIN status and publish WIN to WAMP topic
            if (IsWin)
                ProcessBetWin(sdr);

            // Add bet to cache
            betCache.Add(sdr);
        }
        private void Bet_TimeLenghFinished(object sender, EventArgs e)
        {
            GameBet sdr = sender as GameBet;
            if (sdr == null)
                return;
            //Console.WriteLine("{0}>Bet_TimeLenghFinished={1}", DateTime.Now.ToString("HH:mm:ss.fff"), sdr.Box.TimeLength);

            ProcessBetTimeOut(sdr);


        }
        #endregion


        #region IGameManager

        public void PlaceBet(string userId, string assetPair, string box, decimal bet)
        {
            //Console.WriteLine("{0}> PlaceBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, bet);

            // Get user state
            UserState userState = GetUserState(userId);
            
            // Validate balance
            if (bet > userState.Balance)
                throw new InvalidOperationException("User has no balance for the bet.");

            // TODO: Get Box from... somewhere            
            Box boxObject = Box.FromJson(box);
                        
            // Get Current Coeffs for Game's Assetpair
            CoeffParameters coef = userState.GetParameters(assetPair);
            if (coef == null || 
                coef.PriceSize == 0 || 
                coef.OptionLen==0)
                throw new InvalidOperationException($"Coefficient parameters are not set for Asset Pair [{assetPair}].");

            // Place Bet            
            GameBet newBet = userState.PlaceBet(boxObject, assetPair, bet, coef, quoteFeed);
            newBet.TimeToGraphReached += Bet_TimeToGraphReached;
            newBet.TimeLenghFinished += Bet_TimeLenghFinished;
            
            // Run bet
            newBet.StartWaitTimeToGraph();

            // Save bet to DB
            database.SaveGameBet(userState.UserId, newBet);

            // Set Status, saves User to DB            
            SetUserStatus(userState, GameStatus.BetPlaced, $"BetPlaced[{boxObject.Id}]. AssetPair={assetPair}  Bet={bet}");

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
            SetUserStatus(userState, GameStatus.ParameterChanged, $"ParameterChanged [{pair}] timeToFirstOption={timeToFirstOption}; optionLen={optionLen}; priceSize={priceSize}; nPriceIndex={nPriceIndex}, nTimeIndex={nTimeIndex}");
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
            SetUserStatus(userState, GameStatus.CoeffRequest, $"CoeffRequest [{pair}]");
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


        protected virtual void OnBetWin(BetEventArgs e)
        {
            //Console.WriteLine("{0}>OnBetWin ={1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.Bet.Box.Id);
            BetWin?.Invoke(this, e);
        }
        protected virtual void OnBetLose(BetEventArgs e)
        {
            //Console.WriteLine("{0}>OnBetLose ={1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.Bet.Box.Id);
            BetLose?.Invoke(this, e);
        }


        private class PriceCache
        {
            public decimal CurrentPrice { get; set; }
            public decimal PreviousPrice { get; set; }
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
