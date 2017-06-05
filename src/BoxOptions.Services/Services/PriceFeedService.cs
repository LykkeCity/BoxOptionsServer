using Autofac;
using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
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

        Dictionary<string, InstrumentPrice> lastPrices;


        public PriceFeedService(BoxOptionsSettings settings, IWampHostedRealm realm, IAssetQuoteSubscriber subscriber)
        {
            this.settings = settings;            
            this.subscriber = subscriber;
            lastPrices = new Dictionary<string, InstrumentPrice>();
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
            bool publish = false;            
            if (!lastPrices.ContainsKey(assetBid.Instrument))
            {
                // Asset not in history, add it and set publish = true
                lastPrices.Add(assetBid.Instrument, assetBid);
                publish = true;
            }
            else
            {
                if (lastPrices[assetBid.Instrument].Ask == assetBid.Ask ||
                    lastPrices[assetBid.Instrument].Bid == assetBid.Bid)
                {
                    // One price (Ask or Bid) has not changed. do not publish it
                    // Must only be published when both Ask Bid prices have changed
                    Console.WriteLine("Not Published [{4}]: {0}/{1} -> {2}/{3}",
                        lastPrices[assetBid.Instrument].Ask, lastPrices[assetBid.Instrument].Bid,
                        assetBid.Ask, assetBid.Bid, 
                        assetBid.Instrument);
                    publish = false;
                }
                else
                {
                    // Both prices have changed publish it and assign this new bid to history
                    publish = true;
                    lastPrices[assetBid.Instrument] = assetBid;
                }
            }

            // If assetbid has a zero price (Ask or Bid) do NOT publish it
            if (assetBid.Ask <= 0 || assetBid.Bid <= 0)
            {
                publish = false;
                Console.WriteLine("Not Published [{0}]: Zero Value", assetBid.Instrument);
            }

            // Publish object to WAMP topic
            if (publish)
            {
                Console.WriteLine("Published [{0}]:{1}", assetBid.Instrument, assetBid);
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
            //string json = instrumentBidAskPair.ToJson();
            subject.OnNext(instrumentBidAskPair);
            return Task.FromResult(0);
        }
    }
}
