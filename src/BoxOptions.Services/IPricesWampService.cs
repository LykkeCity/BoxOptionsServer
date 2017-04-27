using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using WampSharp.V2.Realm;

namespace BoxOptions.Services
{
    public class PricesWampService : IStartable, IDisposable
    {
        private readonly BoxOptionsSettings _settings;
        private readonly ILog _log;
        private RabbitMqSubscriber<InstrumentBidAskPair> _subscriber;
        private readonly ISubject<InstrumentBidAskPair> _subject;


        public PricesWampService(
            BoxOptionsSettings settings, 
            IWampHostedRealm realm,
            ILog log)
            
        {
            _settings = settings;
            _log = log;
            _subject = realm.Services.GetSubject<InstrumentBidAskPair>(_settings.BoxOptionsApi.PricesSettings.PricesTopicName);
        }

        public void Start()
        {
            _subscriber = new RabbitMqSubscriber<InstrumentBidAskPair>(new RabbitMqSubscriberSettings
                {
                    ConnectionString = _settings.BoxOptionsApi.PricesSettings.RabbitMqConnectionString,
                    ExchangeName = _settings.BoxOptionsApi.PricesSettings.RabbitMqExchangeName,
                    QueueName = _settings.BoxOptionsApi.PricesSettings.RabbitMqQueueName,
                    IsDurable = _settings.BoxOptionsApi.PricesSettings.RabbitMqIsDurable
                })
                .SetMessageDeserializer(new MessageDeserializer<InstrumentBidAskPair>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy(_settings.BoxOptionsApi.PricesSettings.RabbitMqRoutingKey))
                .SetLogger(_log)
                .Subscribe(ProcessPrice)
                .Start();
        }

        public void Dispose()
        {
            _subscriber.Stop();
        }

        private Task ProcessPrice(InstrumentBidAskPair instrumentBidAskPair)
        {
            _subject.OnNext(instrumentBidAskPair);
            return Task.FromResult(0);
        }
    }
}
