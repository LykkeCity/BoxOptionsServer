using BoxOptions.Core;
using BoxOptions.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTests
{
    
    public class Services
    {
       
        [Fact]
        public void MicrographCacheService_Receive_OK()
        {
            //Arrange
            BoxOptions.Common.BoxOptionsSettings settings = new BoxOptions.Common.BoxOptionsSettings()
            {
                BoxOptionsApi = new BoxOptions.Common.BoxOptionsApiSettings()
                {
                    PricesSettingsBoxOptions = new BoxOptions.Common.PricesSettingsBoxOptions()
                    {
                        GraphPointsCount = 50
                    }
                }
            };
            MockAssetQuoteSubscriber subscriber = new MockAssetQuoteSubscriber();
            MicrographCacheService svc = new MicrographCacheService(settings, subscriber);
            
            //Act
            svc.Start();            
            subscriber.SimMessageReceived();    // Simulate 2 incoming messages for AssetPair [EURUSD]
            var graph = svc.GetGraphData();
            List<string> keys = new List<string>(graph.Keys);

            //Assert
            Assert.Equal(1, graph.Keys.Count);
            Assert.Equal("EURUSD", keys[0]);
            Assert.Equal(2, graph["EURUSD"].Length);
        }

        [Fact]        
        public void MicrographCacheService_Receive_Null()
        {
            //Arrange
            BoxOptions.Common.BoxOptionsSettings settings = new BoxOptions.Common.BoxOptionsSettings()
            {
                BoxOptionsApi = new BoxOptions.Common.BoxOptionsApiSettings()
                {
                    PricesSettingsBoxOptions = new BoxOptions.Common.PricesSettingsBoxOptions()
                    {
                        GraphPointsCount = 50
                    }
                }
            };
            MockAssetQuoteSubscriber subscriber = new MockAssetQuoteSubscriber();
            MicrographCacheService svc = new MicrographCacheService(settings, subscriber);
            Exception ex= null;
            int res = 0;
            
            //Act
            svc.Start();
            subscriber.SimNullMessageReceived();
            res = svc.GetGraphData().Count;
            
            //Assert
            Assert.Equal(0, res);
           
        }

        //[Fact] void PriceFeedService_

        private class MockAssetQuoteSubscriber : BoxOptions.Core.Interfaces.IAssetQuoteSubscriber
        {
            public event EventHandler<InstrumentPrice> MessageReceived;

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
        }
    }
}
