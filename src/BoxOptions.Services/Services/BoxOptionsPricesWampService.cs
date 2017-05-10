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
    public class BoxOptionsPricesWampService: IStartable, IDisposable
    {
        private readonly BoxOptionsSettings _settings;
        private readonly ILog _log;
        /// <summary>
        /// Rabbit MQ Subscriber
        /// </summary>
        private RabbitMqSubscriber<AssetQuote> _subscriber;
        /// <summary>
        /// Wamp Host Publisher
        /// </summary>
        private readonly ISubject<InstrumentBidAskPair> _subject;

        /// <summary>
        /// Bid Cache
        /// </summary>
        private readonly List<AssetPairBid> _assetCache;



        public BoxOptionsPricesWampService(
            BoxOptionsSettings settings,
            IWampHostedRealm realm,
            ILog log)

        {
            _settings = settings;
            _log = log;
            _subject = realm.Services.GetSubject<InstrumentBidAskPair>(_settings.BoxOptionsApi.PricesSettingsBoxOptions.PricesTopicName);

            _assetCache = new List<AssetPairBid>();
        }

        /// <summary>
        /// Subscribe RabbitMq
        /// </summary>
        public void Start()
        {
            _subscriber = new RabbitMqSubscriber<AssetQuote>(new RabbitMqSubscriberSettings
            {
                ConnectionString = _settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOConnectionString,
                ExchangeName = _settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOExchangeName,
                QueueName = _settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOPricesQueueName,
                IsDurable = false
            })
                .SetMessageDeserializer(new MessageDeserializer<AssetQuote>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(_log)
                .Subscribe(ProcessPrice)
                .Start();
        }

        public void Dispose()
        {
            _subscriber.Stop();
        }

        /// <summary>
        /// Publish Data to Wamp Topic "PricesTopicName" when rabbitmq event received
        /// </summary>
        /// <param name="assetQuote"></param>
        /// <returns></returns>
        private Task ProcessPrice(AssetQuote assetQuote)
        {
            // TODO: update or clear asset filtering

            // Filter Asset 
            if (!Common.AllowedAssets.Contains(assetQuote.AssetPair))
            {
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            }

            // Get Asset from cache
            AssetPairBid assetbid = (from a in _assetCache
                                     where a.Id == assetQuote.AssetPair
                                     select a).FirstOrDefault();

            if (assetbid == null)
            {
                // AssetPair is not in cache
                // Add AssetQuote to cache
                assetbid = new AssetPairBid()
                {
                    Id = assetQuote.AssetPair,
                    Date = assetQuote.Timestamp,
                    Ask = assetQuote.IsBuy == Common.ASK ? assetQuote.Price : 0,
                    Bid = assetQuote.IsBuy == Common.ASK ? 0 : assetQuote.Price
                };
                _assetCache.Add(assetbid);
            }
            else
            {
                // AssetPair is in cache
                // Update Bid Quote
                if (assetQuote.IsBuy == Common.ASK)
                    assetbid.Ask = assetQuote.Price;
                else
                    assetbid.Bid = assetQuote.Price;
            }
            
            // TODO: clear date override
            // override asset bid with server UTC date.now
            assetbid.Date = DateTime.UtcNow;

            // If assetbid has Ask and Bid prices, publish to Wamp Topic
            if (assetbid.Ask > 0 && assetbid.Bid > 0)
            {
                InstrumentBidAskPair wampEntry = new InstrumentBidAskPair()
                {
                    Instrument = assetbid.Id,
                    Ask = assetbid.Ask,
                    Bid = assetbid.Bid,
                    Date = assetbid.Date
                };
                
                PublishInstrumentPair(wampEntry);
            }
                
            return Task.FromResult(0);
        }

        private Task PublishInstrumentPair(InstrumentBidAskPair instrumentBidAskPair)
        {
            _subject.OnNext(instrumentBidAskPair);
            //Console.WriteLine($"{instrumentBidAskPair.Date} > {instrumentBidAskPair.Instrument} {instrumentBidAskPair.Bid}/{instrumentBidAskPair.Ask}");
            return Task.FromResult(0);
        }

    }
}
