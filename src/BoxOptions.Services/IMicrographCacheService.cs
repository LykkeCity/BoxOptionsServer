using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Lykke.RabbitMqBroker.Subscriber;

namespace BoxOptions.Services
{
    public interface IMicrographCacheService
    {
        Dictionary<string, GraphBidAskPair[]> GetGraphData();
    }

    public class MicrographCacheService : IMicrographCacheService, IStartable, IDisposable
    {
        private readonly BoxOptionsSettings _settings;
        private RabbitMqSubscriber<InstrumentBidAskPair> _subscriber;
        private readonly Dictionary<string, List<GraphBidAskPair>> _graphQueue;
        private static readonly object GraphQueueLock = new object();

        public MicrographCacheService(BoxOptionsSettings settings)
        {
            _settings = settings;
            _graphQueue = new Dictionary<string, List<GraphBidAskPair>>();
        }

        public void Start()
        {
            _subscriber = new RabbitMqSubscriber<InstrumentBidAskPair>(new RabbitMqSubscriberSettings
                {
                    ConnectionString = _settings.BoxOptionsApi.PricesSettings.RabbitMqConnectionString,
                    ExchangeName = _settings.BoxOptionsApi.PricesSettings.RabbitMqExchangeName,
                    IsDurable = false
                })
                .SetMessageDeserializer(new MessageDeserializer<InstrumentBidAskPair>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy(_settings.BoxOptionsApi.PricesSettings.RabbitMqRoutingKey))
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

        private Task ProcessPrice(InstrumentBidAskPair bidAskPair)
        {
            lock (GraphQueueLock)
            {
                if (!_graphQueue.ContainsKey(bidAskPair.Instrument))
                {
                    _graphQueue.Add(bidAskPair.Instrument, new List<GraphBidAskPair>());
                }

                _graphQueue[bidAskPair.Instrument].Add(new GraphBidAskPair
                {
                    Bid = bidAskPair.Bid,
                    Ask = bidAskPair.Ask,
                    Date = DateTime.UtcNow
                });

                if (_graphQueue[bidAskPair.Instrument].Count > _settings.BoxOptionsApi.PricesSettings.GraphPointsCount)
                {
                    _graphQueue[bidAskPair.Instrument] = _graphQueue[bidAskPair.Instrument]
                        .GetRange(1, _graphQueue[bidAskPair.Instrument].Count - 1);
                }
            }

            return Task.FromResult(0);
        }
    }
}
