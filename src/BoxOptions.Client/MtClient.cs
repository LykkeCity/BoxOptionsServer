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

namespace BoxOptions.Client
{
    public class MtClient
    {
        private string _serverAddress;
        private IWampRealmProxy _realmProxy;
        private IRpcMethods _service;

        IDisposable subscription;

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
            subscription = _realmProxy.Services.GetSubject<InstrumentPrice>("prices.update")
                .Subscribe(info =>
                {
                    if (Program.ShowFeed)
                        Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > BidDate[{info.Date.ToString("yyyy-MM-dd HH:mm:ss")}] | {info.Instrument} {info.Bid}/{info.Ask}");
                });


            //Console.ReadLine();            
        }
        public void Stop()
        {
            subscription.Dispose();
        }

        internal void Launch(string userId)
        {
            string result = _service.Launch(userId);
            Console.WriteLine("{0}> Game Launch({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        }

        internal void Wake(string userId)
        {
            string result = _service.Wake(userId);
            Console.WriteLine("{0}> Wake({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        }

        internal void Sleep(string userId)
        {
            string result = _service.Sleep(userId);
            Console.WriteLine("{0}> Sleep({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        }

        internal void GameStart(string userId, string assetPair)
        {
            string result = _service.GameStart(userId, assetPair);
            Console.WriteLine("{0}> GameStart({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair, result);
        }

        internal void GameClose(string userId)
        {
            string result = _service.GameClose(userId);
            Console.WriteLine("{0}> GameClose({1}) = {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, result);
        }

        internal void PlaceBet(string userId, string box, decimal betAmount)
        {
            string result = _service.PlaceBet(userId, box, betAmount);
            Console.WriteLine("{0}> PlaceBet({1},{2},{3}) = {4}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount, result);
        }

        internal void ChangeBet(string userId, string box, decimal betAmount)
        {
            string result = _service.ChangeBet(userId, box, betAmount);
            Console.WriteLine("{0}> ChangeBet({1},{2},{3}) = {4}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount, result);
        }

        internal void ChangeScale(string userId, decimal scale)
        {
            string result = _service.ChangeScale(userId, scale);
            Console.WriteLine("{0}> ChangeBet({1},{2}) = {3}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, scale, result);
        }


        #endregion
    }
}
