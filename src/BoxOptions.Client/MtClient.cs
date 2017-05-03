using System;
using System.Collections.Generic;
using System.Threading;
using BoxOptions.Core;
using BoxOptions.Services;
using WampSharp.V2;
using WampSharp.V2.Client;

namespace BoxOptions.Client
{
    public class MtClient
    {
        private string _serverAddress;
        private IWampRealmProxy _realmProxy;
        private IRpcMethodsService _service;

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
                catch
                {
                    Console.WriteLine("Retrying in 5 sec...");
                    Thread.Sleep(5000);
                }
            }
            Console.WriteLine($"Connected to server {_serverAddress}");

            _realmProxy = channel.RealmProxy;
            _service = _realmProxy.Services.GetCalleeProxy<IRpcMethodsService>();
        }

        public void SetEnv(ClientEnv env)
        {
            switch (env)
            {
                case ClientEnv.Local:
                    _serverAddress = "ws://localhost:5000/ws";
                    break;
                case ClientEnv.Prod:
                    _serverAddress = "ws://boxoptions-api.lykke.com:5000/ws";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Rpc methods

        public AssetPair[] GetAssets()
        {
            var data = _service.InitAssets();
            return data;
        }

        public Dictionary<string, GraphBidAskPair[]> GetChardData()
        {
            var data = _service.InitChardData();
            return data;
        }

        public void Prices()
        {
            IDisposable subscription = _realmProxy.Services.GetSubject<InstrumentBidAskPair>("prices.update")
                .Subscribe(info =>
                {
                    Console.WriteLine($"{info.Instrument} {info.Bid}/{info.Ask}");
                });


            Console.ReadLine();
            subscription.Dispose();
        }

        #endregion
    }
}
