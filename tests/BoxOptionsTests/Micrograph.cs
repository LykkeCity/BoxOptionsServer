using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using BoxOptions.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Tests
{
    [TestFixture]
    public class Micrograph
    {
        BoxOptions.Common.Settings.BoxOptionsApiSettings _settings;
        MockAssetQuoteSubscriber _subscriber;
        //MicrographCacheService _service;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _settings = new BoxOptions.Common.Settings.BoxOptionsApiSettings
            {
                PricesSettingsBoxOptions = new BoxOptions.Common.Settings.PricesSettingsBoxOptions
                {
                    GraphPointsCount = 50
                }
            };
            _subscriber = new MockAssetQuoteSubscriber();
            
        }
        [Test]
        [Category("MicrographCacheService")]
        public void MicrographCacheService_Receive_OK()
        {
            // Arrange
            MicrographCacheService service = new MicrographCacheService(_settings, _subscriber);

            // Act
            service.Start();
            _subscriber.SimMessageReceived();    // Simulate 2 incoming messages for AssetPair [EURUSD]
            var graph = service.GetGraphData();
            List<string> keys = new List<string>(graph.Keys);

            // Assert
            Assert.AreEqual(1, graph.Keys.Count);
            Assert.AreEqual("EURUSD", keys[0]);
            Assert.AreEqual(2, graph["EURUSD"].Length);
        }

        [Test]
        [Category("MicrographCacheService")]
        public void MicrographCacheService_Receive_Null()
        {
            // Arrange
            MicrographCacheService service = new MicrographCacheService(_settings, _subscriber);
            int res = 0;

            // Act
            service.Start();
            _subscriber.SimNullMessageReceived();
            res = service.GetGraphData().Count;

            // Assert
            Assert.AreEqual(0, res);
        }
        
        private class MockAssetQuoteSubscriber : IAssetQuoteSubscriber
        {
            public event EventHandler<IInstrumentPrice> MessageReceived;


            public void Dispose()
            {
            }
            public void Start()
            {
            }
            public void SimMessageReceived()
            {
                MessageReceived?.Invoke(this, new InstrumentPrice() { Instrument = "EURUSD", Date = DateTime.UtcNow, Ask = 1.245d, Bid = 1.231 });
                MessageReceived?.Invoke(this, new InstrumentPrice() { Instrument = "EURUSD", Date = DateTime.UtcNow, Ask = 1.243d, Bid = 1.235 });
            }

            public void SimNullMessageReceived()
            {
                MessageReceived?.Invoke(this, null);
            }

            public Task<bool> ReloadAssetConfiguration()
            {
                throw new NotImplementedException();
            }
        }
    }
}
