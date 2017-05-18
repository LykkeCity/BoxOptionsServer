using System;
using System.Collections.Generic;
using System.Threading;
using BoxOptions.Core;
using BoxOptions.Services;
using WampSharp.V2;
using WampSharp.V2.Client;
using System.Net;
using System.IO;

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
                    Console.WriteLine($"UTC[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] > BidDate[{info.Date.ToString("yyyy-MM-dd HH:mm:ss")}] | {info.Instrument} {info.Bid}/{info.Ask}");
                });


            //Console.ReadLine();            
        }
        public void Stop()
        {
            subscription.Dispose();
        }

        #endregion
    }
}
