using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Lykke.RabbitMqBroker.Subscriber;
using WampSharp.V2.Realm;

namespace BoxOptions.Services
{
    public class PricesWampService : IStartable, IDisposable
    {
        private readonly BoxOptionsSettings _settings;
        private RabbitMqSubscriber<InstrumentBidAskPair> _subscriber;
        private readonly ISubject<InstrumentBidAskPair> _subject;


        public PricesWampService(BoxOptionsSettings settings,
            IWampHostedRealm realm)
        {
            _settings = settings;
            _subject = realm.Services.GetSubject<InstrumentBidAskPair>(_settings.PricesTopicName);
        }

        public void Start()
        {
            _subscriber = new RabbitMqSubscriber<InstrumentBidAskPair>(new RabbitMqSubscriberSettings
                {
                    ConnectionString = _settings.RabbitMqConnectionString,
                    ExchangeName = _settings.RabbitMqExchangeName,
                    QueueName = _settings.RabbitMqQueueName,
                    IsDurable = _settings.RabbitMqIsDurable
                })
                .SetMessageDeserializer(new MessageDeserializer<InstrumentBidAskPair>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy(_settings.RabbitMqRoutingKey))
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
