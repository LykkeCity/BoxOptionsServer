using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;

namespace BoxOptions.Services
{
    

    public class BoxOptionsMicrographCacheService : IMicrographCacheService, IStartable, IDisposable
    {
        private readonly BoxOptionsSettings _settings;
        private readonly ILog _log;
        private RabbitMqSubscriber<AssetQuote> _subscriber;
        private readonly Dictionary<string, List<GraphBidAskPair>> _graphQueue;
        private readonly List<AssetPairBid> _assetCache;
        private static readonly object GraphQueueLock = new object();
                        
        public BoxOptionsMicrographCacheService(
            BoxOptionsSettings settings,
            ILog log)
        {
            _settings = settings;
            _log = log;
            _graphQueue = new Dictionary<string, List<GraphBidAskPair>>();
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
                QueueName = _settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOMicrographQueueName,
                IsDurable = false
            })
                .SetMessageDeserializer(new MessageDeserializer<AssetQuote>())
                //.SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy(_settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqMicrographRoutingKey))                
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(_log)
                .Subscribe(ProcessPrice)
                .Start();
        }

        public void Dispose()
        {            
            _subscriber.Stop();
        }

        public Dictionary<string, GraphBidAskPair[]> GetGraphData()
        {
            lock (GraphQueueLock)
            {
                var copy = new Dictionary<string, GraphBidAskPair[]>();

                foreach (var pair in _graphQueue)
                {
                    var pairs = new List<GraphBidAskPair>();

                    foreach (var bidAsk in pair.Value)
                    {
                        pairs.Add(new GraphBidAskPair
                        {
                            Ask = bidAsk.Ask,
                            Bid = bidAsk.Bid,
                            Date = bidAsk.Date
                        });
                    }

                    copy.Add(pair.Key, pairs.ToArray());
                }

                return copy;
            }
        }

        /// <summary>
        /// Update graph data when rabbitmq event received
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


            lock (GraphQueueLock)
            {
                // Get Asset from cache
                AssetPairBid assetbid= (from a in _assetCache
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

                // If assetbid has Ask and Bid prices, add it to Graphdata
                if (assetbid.Ask > 0 && assetbid.Bid > 0)
                {
                    // Add AssetPair to graph;
                    if (!_graphQueue.ContainsKey(assetbid.Id))
                    {
                        _graphQueue.Add(assetbid.Id, new List<GraphBidAskPair>());
                    }

                    _graphQueue[assetbid.Id].Add(new GraphBidAskPair
                    {
                        Bid = assetbid.Bid,
                        Ask = assetbid.Ask,
                        Date = assetbid.Date
                    });

                    if (_graphQueue[assetbid.Id].Count > _settings.BoxOptionsApi.PricesSettingsBoxOptions.GraphPointsCount)
                    {
                        _graphQueue[assetbid.Id] = _graphQueue[assetbid.Id]
                            .GetRange(1, _graphQueue[assetbid.Id].Count - 1);
                    }
                }
            }

            return Task.FromResult(0);
        }


    }
}
