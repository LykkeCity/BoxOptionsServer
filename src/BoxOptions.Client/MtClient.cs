using System;
using System.Collections.Generic;
using System.Threading;
using BoxOptions.Core;
using BoxOptions.Services;
using WampSharp.V2;
using WampSharp.V2.Client;
using System.Net;
using System.IO;
using BoxOptions.Core.Models;
using Lykke.Common;
using BoxOptions.Services.Models;
using System.Threading.Tasks;
using System.Linq;

namespace BoxOptions.Client
{
    public class MtClient
    {
        private string _serverAddress;
        private IWampRealmProxy _realmProxy;
        private IRpcMethods _service;

        IDisposable subscription1;
        IDisposable subscription2;

        public void Connect(ClientEnv env)
        {
            SetEnv(env);
            var factory = new DefaultWampChannelFactory();
            IWampChannel channel = factory.CreateJsonChannel(_serverAddress, "box-options");

            while (!channel.RealmProxy.Monitor.IsConnected)
            {
                try
                {
                    Console.WriteLine($"Trying to connect to server {_serverAddress}...");
                    channel.Open().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex);
                    Console.WriteLine("Retrying in 5 sec...");
                    Thread.Sleep(5000);
                }
            }
            Console.WriteLine($"Connected to server {_serverAddress}");

            _realmProxy = channel.RealmProxy;
            _service = _realmProxy.Services.GetCalleeProxy<IRpcMethods>();
        }

        public void SetEnv(ClientEnv env)
        {
            switch (env)
            {
                case ClientEnv.Local:
                    // kestrel Port
                    _serverAddress = "ws://localhost:5000/ws";
                    // IIS Port from launchSettings.json
                    //_serverAddress = "ws://127.0.0.1:59136/ws";
                    break;
                case ClientEnv.Dev:
                    // kestrel Port
                    _serverAddress = "ws://13.93.116.252:5050/ws";
                    break;
                case ClientEnv.Prod:
                    _serverAddress = "ws://boxoptions-api.lykke.com:5000/ws";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async void PostLog()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/api/Log");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            Stream httpstream = await httpWebRequest.GetRequestStreamAsync();
            using (var streamWriter = new StreamWriter(httpstream))
            {
                string json = "{" +
                    "\"ClientId\": \"SomeId\"," +
                    "\"EventCode\": \"0\"," +
                    "\"Message\": \"Test\"" +
                    "}";


                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Dispose();
            }

            WebResponse httpResponse = await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine("Res: {0}", result);
            }

        }

        public List<BetInfo> RunningBets { get; private set; } = null;

        #region Rpc methods

        public AssetPair[] GetAssets()
        {
            var data = _service.InitAssets();
            return data;
        }

        public Dictionary<string, Price[]> GetChardData()
        {
            var data = _service.InitChartData();
            return data;
        }

        public void Prices()
        {
            subscription1 = _realmProxy.Services.GetSubject<InstrumentPrice>("prices.update")
                .Subscribe(OnPriceFeed);
            //info =>
            //{
            //    if (Program.ShowFeed)
            //        Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > BidDate[{info.Date.ToString("yyyy-MM-dd HH:mm:ss")}] | {info.Instrument} {info.Bid}/{info.Ask}");
            //});
        }
        void OnPriceFeed(InstrumentPrice info)
        {
            if (Program.ShowFeed)
                Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > BidDate[{info.Date.ToString("yyyy-MM-dd HH:mm:ss")}] | {info.Instrument} {info.Bid}/{info.Ask}");
        }

        public void SubscribeGameEvents()
        {
            if (subscription2 != null)
            {
                subscription2.Dispose();
                subscription2 = null;
            }
            subscription2 = _realmProxy.Services.GetSubject<GameEvent>("game.events." + Program.UserId)
                .Subscribe(OnGameResult);
            //info =>
            //{
            //    Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > INFO[{info.ToJson()}");
            //});
        }
        void OnGameResult(GameEvent info)
        {
            Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > Type={info.EventType} pars={info.EventParameters}");
            if (RunningBets  != null && info.EventType == 1)
            {
                BetInfo binfo = (from b in RunningBets
                                 where info.EventParameters.Contains(b.BetId)
                                 select b).FirstOrDefault();
                if (binfo == null)
                    throw new InvalidOperationException("bet not in cache");

                binfo.Events.Add(info.EventParameters);

            }
        }

        public void Stop()
        {
            subscription1.Dispose();
            subscription2.Dispose();
        }
        public string InitUser(string userId)
        {
            var data = _service.InitUser(userId);
            return data;
        }

        internal void PlaceBet(string userId, string assetpair, string box, decimal betAmount)
        {
            var result = _service.PlaceBet(userId, assetpair, box, betAmount);
            Console.WriteLine("{0}> PlaceBet({1},{2},{3}) = {4}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount, result.Status);
        }

      


        internal void GetBalance(string userId)
        {
            decimal result = _service.GetBalance(userId);
            Console.WriteLine("{0}> GetBalance({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        }
        internal void SetBalance(string userId, decimal newBalance)
        {
            string result = _service.SetBalance(userId, newBalance);
            Console.WriteLine("{0}> SetBalance({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, newBalance, result);
        }

      
        internal void RequestCoeff(string userId, string pair)
        {
            //_service.SaveLog(userId, ((int)GameStatus.CoeffRequest).ToString(), $"Coeff Request: [{pair}]");
            var res = _service.RequestCoeff(userId, pair);
            Console.WriteLine(res);
        }
        internal void RequestCoeffs(string userId, string pair)
        {
            //_service.SaveLog(userId, ((int)GameStatus.CoeffRequest).ToString(), $"Coeff Request: [{pair}]");
            for (int i = 0; i < 20; i++)
            {
                var res = _service.RequestCoeff(userId, pair);
                Console.WriteLine("{0} > {1}", i, res);
                System.Threading.Thread.Sleep(999);
            }
        }
        int BetCtr = 20;
        internal void PlaceBets(string userId)
        {
            RunningBets = new List<BetInfo>();

            // {"BoxId":"4E8F0395-7DB5-440F-B434-49217CF9DA89","MinPrice":0.9649558795483333,"MaxPrice":0.9650041204516666,"TimeToGraph":32.0,"TimeLength":6.999999999999992,"Coefficient":1.027643619053309,"BetState":3,"PreviousPrice":{"Instrument":"USDCHF","Bid":0.96499,"Ask":0.96503,"Date":"2017-06-08T04:03:17.673696Z","Time":1496894597673},"CurrentPrice":{"Instrument":"USDCHF","Bid":0.96496,"Ask":0.965,"Date":"2017-06-08T04:03:22.778847Z","Time":1496894602778},"TimeToGraphStamp":"2017-06-08T04:04:42.819815Z","WinStamp":"2017-06-08T04:04:42.819902Z","FinishedStamp":null,"Timestamp":"2017-06-08T04:04:11.208345Z","BetAmount":1.0,"IsWin":true}
            string boxstring = "{{" +
                "\"Id\":\"{0}\"," +
                "\"Coefficient\":{1}," +
                "\"MinPrice\":0.9649558795483333," +
                "\"MaxPrice\":0.9650041204516666," +
                "\"TimeToGraph\":4," +
                "\"TimeLength\":7" +
                "}}";
            // place 20 bets concurrently
            double coef = 1.027643619053309;
            System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");
            
            for (int i = 0; i < BetCtr; i++)
            {
                Task.Run(() =>
                {
                    
                    
                    string GUID = Guid.NewGuid().ToString().ToLower();
                    BetInfo b = new BetInfo(GUID);
                    try
                    {
                        string box = string.Format(CI, boxstring, GUID, coef);
                        coef += 0.02;
                        Console.WriteLine("{0} | {1} > Placing Bet", DateTime.UtcNow.ToString("HH:mm:ss.fff"), GUID);
                        PlaceBetResult result = _service.PlaceBet(userId, "EURCHF", box, 1);
                        Console.WriteLine("{0} | {1} > Result = {2} ({3})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), GUID, result.BetTimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff"), result.Status);
                        b.Result = result;
                        RunningBets.Add(b);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
                Thread.Sleep(10);
            }
        }
        internal void CheckBets()
        {
            int ctr = 0;
            foreach (var bet in RunningBets)
            {                
                Console.WriteLine("{0} > {1} has {2} events", ++ctr, bet.BetId, bet.Events.Count);
            }
        }
        #endregion

        public class BetInfo
        {
            public string BetId { get; private set; }
            public PlaceBetResult Result { get; set; }

            public List<string> Events { get; set; }

            public BetInfo(string betId)
            {
                BetId = betId;
                Events = new List<string>();
            }
            
        }

        //internal void Launch(string userId)
        //{
        //    string result = _service.Launch(userId);
        //    Console.WriteLine("{0}> Game Launch({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        //}

        //internal void Wake(string userId)
        //{
        //    string result = _service.Wake(userId);
        //    Console.WriteLine("{0}> Wake({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        //}

        //internal void Sleep(string userId)
        //{
        //    string result = _service.Sleep(userId);
        //    Console.WriteLine("{0}> Sleep({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        //}

        //internal void GameStart(string userId, string assetPair)
        //{
        //    string result = _service.GameStart(userId, assetPair);
        //    Console.WriteLine("{0}> GameStart({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair, result);
        //}

        //internal void GameClose(string userId)
        //{
        //    string result = _service.GameClose(userId);
        //    Console.WriteLine("{0}> GameClose({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        //}

        //internal void ChangeBet(string userId, string box, decimal betAmount)
        //{
        //    string result = _service.ChangeBet(userId, box, betAmount);
        //    Console.WriteLine("{0}> ChangeBet({1},{2},{3}) = {4}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount, result);
        //}

        //internal void ChangeScale(string userId, decimal scale)
        //{
        //    string result = _service.ChangeScale(userId, scale);
        //    Console.WriteLine("{0}> ChangeBet({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, scale, result);
        //}
    }
}
