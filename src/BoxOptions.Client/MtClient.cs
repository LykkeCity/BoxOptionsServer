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

       
      
        public void Stop()
        {
            subscription1.Dispose();            
        }
       

        internal void ChangeParameter(string userId, string assetPair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            string result = _service.ChangeParameters(userId, assetPair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            Console.WriteLine("{0}> ChangeParameter({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair, result);
        }       
       

        internal void RequestCoeffs(string userId)
        {
            string[] AllowedAssets = new string[] { "EURUSD", "EURCHF", "EURGBP", "EURJPY", "USDCHF" };
            //string[] AllowedAssets = new string[] { "EURUSD", "EURGBP" };

            foreach (var asset in AllowedAssets)
            {
                Task.Run(() =>
                {
                    Console.WriteLine("1st Request");
                    RequestCoeff(userId, asset);
                                        
                    Console.WriteLine("2nd Request Same pars");
                    RequestCoeff(userId, asset);


                    Console.WriteLine("{0}>{1} | Change Parameters", DateTime.Now.ToString("HH:mm:ss.fff"), asset);
                    Random r = new Random();                    
                    _service.ChangeParameters(userId, asset, r.Next(50000), r.Next(8000), 0.0003d, 8, 9);

                    Console.WriteLine("3rd Request diff pars");                    
                    RequestCoeff(userId, asset);
                });
            }

        }
        internal void RequestCoeff(string userId, string asset)
        {
            string tasset = asset;
            Console.WriteLine("{0}>{1} | START ({2})", DateTime.Now.ToString("HH:mm:ss.fff"), tasset, Thread.CurrentThread.ManagedThreadId);
            var res = _service.RequestCoeff(userId, asset);
            string trimmedRes = "NULL";
            if (res.Length > 20)
                trimmedRes = res.Substring(0, 20);
            Console.WriteLine("{0}>{1} | FINISHED ({2}) Res={3}", DateTime.Now.ToString("HH:mm:ss.fff"), tasset, Thread.CurrentThread.ManagedThreadId, trimmedRes);
        }


        #endregion


    }
}
