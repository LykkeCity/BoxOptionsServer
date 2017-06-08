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
            subscription2 = _realmProxy.Services.GetSubject<BetResult>("game.events." + Program.UserId)
                .Subscribe(OnGameResult);
            //info =>
            //{
            //    Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > INFO[{info.ToJson()}");
            //});
        }
        void OnGameResult(BetResult info)
        {
            Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > INFO[{info.ToJson()}");
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

        internal void ChangeParameter(string userId, string assetPair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            string result = _service.ChangeParameters(userId, assetPair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            Console.WriteLine("{0}> ChangeParameter({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair, result);
        }
        internal void GetParameter(string userId, string assetPair)
        {
            var res = _service.GetParameters(userId, assetPair);
            Console.WriteLine("\tAssetPair:{0}", res.AssetPair);
            Console.WriteLine("\tTimeToFirstOption:{0}", res.TimeToFirstOption);
            Console.WriteLine("\tOptionLen:{0}", res.OptionLen);
            Console.WriteLine("\tPriceSize:{0}", res.PriceSize);
            Console.WriteLine("\tNPriceIndex:{0}", res.NPriceIndex);
            Console.WriteLine("\tNTimeIndex:{0}", res.NTimeIndex);

        }

        internal void RequestCoeff(string userId, string pair)
        {
            var res = _service.RequestCoeff(userId, pair);
            Console.WriteLine(res);
        }

        internal void PlaceBets(string userId)
        {
         

            // {"BoxId":"4E8F0395-7DB5-440F-B434-49217CF9DA89","MinPrice":0.9649558795483333,"MaxPrice":0.9650041204516666,"TimeToGraph":32.0,"TimeLength":6.999999999999992,"Coefficient":1.027643619053309,"BetState":3,"PreviousPrice":{"Instrument":"USDCHF","Bid":0.96499,"Ask":0.96503,"Date":"2017-06-08T04:03:17.673696Z","Time":1496894597673},"CurrentPrice":{"Instrument":"USDCHF","Bid":0.96496,"Ask":0.965,"Date":"2017-06-08T04:03:22.778847Z","Time":1496894602778},"TimeToGraphStamp":"2017-06-08T04:04:42.819815Z","WinStamp":"2017-06-08T04:04:42.819902Z","FinishedStamp":null,"Timestamp":"2017-06-08T04:04:11.208345Z","BetAmount":1.0,"IsWin":true}
            string boxstring = "{{" +
                "\"Id\":\"{0}\"," +
                "\"Coefficient\":{1}," +
                "\"MinPrice\":0.9649558795483333," +
                "\"MaxPrice\":0.9650041204516666," +
                "\"TimeToGraph\":32," +
                "\"TimeLength\":6.9999999999999973" +                
                "}}";
            // place 20 bets concurrently
            double coef = 1.027643619053309;
            System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");
            List<PlaceBetResult> results = new List<PlaceBetResult>();
            for (int i = 0; i < 100; i++)
            {
                Task.Run(()=>
                {
                    string GUID = Guid.NewGuid().ToString().ToLower();
                    try
                    {
                        string box = string.Format(CI, boxstring, GUID, coef);
                        coef += 0.02;
                        Console.WriteLine("{0} | {1} > Placing Bet", DateTime.UtcNow.ToString("HH:mm:ss.fff"), GUID);
                        PlaceBetResult result = _service.PlaceBet(userId, "USDCHF", box, 1);
                        Console.WriteLine("{0} | {1} > Result = {2} ({3})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), GUID, result.BetTimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff"), result.Status);                        
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
                Thread.Sleep(10);
            }

            Console.WriteLine(results.Count);

        }

        #endregion


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
