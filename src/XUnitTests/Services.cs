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
        public void AssetQuoteSubscriber_construct()
        {
            BoxOptions.Common.BoxOptionsSettings settings = new BoxOptions.Common.BoxOptionsSettings();
            AssetQuoteSubscriber acs = new AssetQuoteSubscriber(settings);
            acs.MessageReceived += Acs_MessageReceived;
            acs.Start();

            acs.Dispose();
        }

        private void Acs_MessageReceived(object sender, AssetPairBid e)
        {
            
        }
    }
}
