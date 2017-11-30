using Autofac;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Common.Models;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using WampSharp.V2.Realm;
using BoxOptions.Core.Interfaces;

namespace BoxOptions.Services
{
    public class PriceFeedService: IStartable, IDisposable
    {
        /// <summary>
        /// Settings Object        
        /// </summary>
        private readonly BoxOptionsApiSettings settings;
        
        /// <summary>
        /// Wamp Subject Publisher
        /// </summary>
        private readonly ISubject<IInstrumentPrice> subject;
        /// <summary>
        /// Asset Quote Subscriber
        /// </summary>
        IAssetQuoteSubscriber subscriber;
        

        public PriceFeedService(BoxOptionsApiSettings settings, IWampHostedRealm realm, IAssetQuoteSubscriber subscriber)
        {
            this.settings = settings;            
            this.subscriber = subscriber;            
            subject = realm.Services.GetSubject<IInstrumentPrice>(this.settings.PricesSettingsBoxOptions.PricesTopicName);         
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
        private async void Subscriber_MessageReceived(object sender, IInstrumentPrice e)
        {
            await ProcessPrice(e);
        }

        /// <summary>
        /// Process AssetQuote received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="assetQuote"></param>
        /// <returns></returns>
        private Task ProcessPrice(IInstrumentPrice assetBid)
        {         
            // Parameter validation
            if (assetBid == null ||
                string.IsNullOrEmpty(assetBid.Instrument) ||
                assetBid.Ask <= 0 ||
                assetBid.Bid <= 0)
            {
                return Task.FromResult(0);
            }

            PublishInstrumentPair(assetBid);
            return Task.FromResult(0);

        }

        /// <summary>
        /// Publish Intrument Bid/Ask pair to WAMP topic
        /// </summary>
        /// <param name="instrumentBidAskPair"></param>
        /// <returns></returns>
        private Task PublishInstrumentPair(IInstrumentPrice instrumentBidAskPair)
        {
            //string json = instrumentBidAskPair.ToJson();
            subject.OnNext(instrumentBidAskPair);
            return Task.FromResult(0);
        }
    }
}
