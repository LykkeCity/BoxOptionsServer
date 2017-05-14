using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    
    public class AssetQuoteSubscriber : IAssetQuoteSubscriber
    {
        /// <summary>
        /// Settings Object
        /// </summary>
        private readonly BoxOptionsSettings settings;
        /// <summary>
        /// Incoming Asset Cache
        /// </summary>
        private readonly List<AssetPairBid> assetCache;

        /// <summary>
        /// RabbitMQ Subscriber
        /// </summary>
        private RabbitMqSubscriber<AssetQuote> subscriber;

        /// <summary>
        /// Logger Object
        /// </summary>
        ILog log;

        /// <summary>
        /// Thrown when a new message is received from RabbitMQ Queue
        /// </summary>
        public event EventHandler<AssetPairBid> MessageReceived;

        public AssetQuoteSubscriber(BoxOptionsSettings settings, ILog log)
        {
            assetCache = new List<AssetPairBid>();
            this.settings = settings;
            this.log = log;
        }

        public void Start()
        {
            subscriber = new RabbitMqSubscriber<AssetQuote>(new RabbitMqSubscriberSettings
            {
                ConnectionString = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOConnectionString,
                ExchangeName = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOExchangeName,
                QueueName = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOPricesQueueName,
                IsDurable = false
            })
               .SetMessageDeserializer(new MessageDeserializer<AssetQuote>())
               .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
               .SetLogger(log)
               .Subscribe(ProcessMessage)
               .Start();
        }
        public void Dispose()
        {
            subscriber.Stop();
        }

        private Task ProcessMessage(AssetQuote assetQuote)
        {
            //Console.WriteLine("AssetQuoteSubscriber: {0} > {1} | {2}[{3}]", assetQuote.AssetPair, assetQuote.Timestamp, assetQuote.IsBuy, assetQuote.Price);
            
            // TODO: update or clear asset filtering
            // Filter Asset 
            if (!Common.AllowedAssets.Contains(assetQuote.AssetPair))
            {
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            }
            else
            {
                // Asset allowed, add it to cache and
                // invoke MessageReceived event


                // Get Asset from cache
                AssetPairBid assetbid = (from a in assetCache
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
                    assetCache.Add(assetbid);
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

                MessageReceived?.Invoke(this, assetbid);
                return Task.FromResult(0);
            }
        }
    }
}
