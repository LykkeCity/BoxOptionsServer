﻿using Autofac;
using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using WampSharp.V2.Realm;

namespace BoxOptions.Services
{
    public class PriceFeedService: IStartable, IDisposable
    {
        /// <summary>
        /// Settings Object        
        /// </summary>
        private readonly BoxOptionsSettings settings;
        
        /// <summary>
        /// Wamp Subject Publisher
        /// </summary>
        private readonly ISubject<InstrumentPrice> subject;
        /// <summary>
        /// Asset Quote Subscriber
        /// </summary>
        IAssetQuoteSubscriber subscriber;
       



        public PriceFeedService(BoxOptionsSettings settings, IWampHostedRealm realm, IAssetQuoteSubscriber subscriber)
        {
            this.settings = settings;            
            this.subscriber = subscriber;

            subject = realm.Services.GetSubject<InstrumentPrice>(this.settings.BoxOptionsApi.PricesSettingsBoxOptions.PricesTopicName);         
        }

        /// <summary>
        /// Start receiving events from IAssetQuoteSubscriber
        /// </summary>
        public void Start()
        {
            subscriber.MessageReceived += Subscriber_MessageReceived;
        }
        /// <summary>
        /// Dispose PriceFeedService Instance
        /// </summary>
        public void Dispose()
        {
            subscriber.MessageReceived -= Subscriber_MessageReceived;
        }

        /// <summary>
        /// Event Handler for messages received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Subscriber_MessageReceived(object sender, InstrumentPrice e)
        {
            await ProcessPrice(e);
        }

        /// <summary>
        /// Process AssetQuote received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="assetQuote"></param>
        /// <returns></returns>
        private Task ProcessPrice(InstrumentPrice assetBid)
        {
            //Console.WriteLine("{4}>> PriceFeedService: {0} > {1} | {2}/{3}", assetBid.Id, assetBid.Date, assetBid.Bid, assetBid.Ask,DateTime.Now.ToString("HH:mm:ss.fff"));

            // Parameter validation
            if (assetBid == null ||
                string.IsNullOrEmpty(assetBid.Instrument) ||
                assetBid.Ask <= 0 ||
                assetBid.Bid <= 0)
            {
                return Task.FromResult(0);
            }

            // If assetbid has Ask and Bid prices, publish to Wamp Topic
            if (assetBid.Ask > 0 && assetBid.Bid > 0)
            {
                // Publish object to WAMP topic
                PublishInstrumentPair(assetBid);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Publish Intrument Bid/Ask pair to WAMP topic
        /// </summary>
        /// <param name="instrumentBidAskPair"></param>
        /// <returns></returns>
        private Task PublishInstrumentPair(InstrumentPrice instrumentBidAskPair)
        {
            subject.OnNext(instrumentBidAskPair);
            return Task.FromResult(0);
        }
    }
}
