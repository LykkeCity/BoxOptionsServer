using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using WampSharp.V2.Realm;
using System.Collections.Generic;

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
        private readonly ISubject<InstrumentBidAskPair> subject;
        /// <summary>
        /// Asset Quote Subscriber
        /// </summary>
        IAssetQuoteSubscriber subscriber;
       



        public PriceFeedService(BoxOptionsSettings settings, IWampHostedRealm realm, IAssetQuoteSubscriber subscriber)
        {
            this.settings = settings;            
            this.subscriber = subscriber;

            subject = realm.Services.GetSubject<InstrumentBidAskPair>(this.settings.BoxOptionsApi.PricesSettingsBoxOptions.PricesTopicName);         
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
        private async void Subscriber_MessageReceived(object sender, AssetPairBid e)
        {
            await ProcessPrice(e);
        }

        /// <summary>
        /// Process AssetQuote received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="assetQuote"></param>
        /// <returns></returns>
        private Task ProcessPrice(AssetPairBid assetBid)
        {
            //Console.WriteLine("{4}>> PriceFeedService: {0} > {1} | {2}/{3}", assetBid.Id, assetBid.Date, assetBid.Bid, assetBid.Ask,DateTime.Now.ToString("HH:mm:ss.fff"));

            // Parameter validation
            if (assetBid == null ||
                string.IsNullOrEmpty(assetBid.Id) ||
                assetBid.Ask <= 0 ||
                assetBid.Bid <= 0)
            {
                return Task.FromResult(0);
            }

            // If assetbid has Ask and Bid prices, publish to Wamp Topic
            if (assetBid.Ask > 0 && assetBid.Bid > 0)
            {
                // Create WAMP topic object
                InstrumentBidAskPair wampEntry = new InstrumentBidAskPair()
                {
                    Instrument = assetBid.Id,
                    Ask = assetBid.Ask,
                    Bid = assetBid.Bid,
                    Date = assetBid.Date
                };

                // Publish object to WAMP topic
                PublishInstrumentPair(wampEntry);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Publish Intrument Bid/Ask pair to WAMP topic
        /// </summary>
        /// <param name="instrumentBidAskPair"></param>
        /// <returns></returns>
        private Task PublishInstrumentPair(InstrumentBidAskPair instrumentBidAskPair)
        {
            subject.OnNext(instrumentBidAskPair);
            return Task.FromResult(0);
        }
    }
}
