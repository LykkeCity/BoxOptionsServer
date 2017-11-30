using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace BoxOptions.CoefficientCalculator
{
    public class Calculator : ICoefficientCalculator, IDisposable
    {   
        private bool isSubscriberRunning;
        private IAssetQuoteSubscriber quoteSubscriber;
        private IAssetDatabase historyRep;

        private bool isDisposing = false;

        public Calculator(IAssetQuoteSubscriber quoteSubscriber, IHistoryHolder historyRep)
        {
            isSubscriberRunning = false;            
        }

        

        public void StartSubscriber()
        {   
            if (quoteSubscriber == null)
                throw new InvalidOperationException("QuoteSubscriber not available");
                        
            quoteSubscriber.MessageReceived += QuoteSubscriber_MessageReceived;
            isSubscriberRunning = true;
        }        
        public void StopSubscriber()
        {   
            if (quoteSubscriber == null)
                throw new InvalidOperationException("QuoteSubscriber not available");

            quoteSubscriber.MessageReceived -= QuoteSubscriber_MessageReceived;
            isSubscriberRunning = false;
        }


        public Task<string> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex, string userId)
        {
            throw new NotImplementedException();
        }
                
        public Task<string> RequestAsync(string pair, string userId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (isDisposing)
                return;
            isDisposing = true;
            
            if (isSubscriberRunning)
                StopSubscriber();
            quoteSubscriber = null;
        }

        private Task ProcessPrice(IInstrumentPrice assetBid)
        {
            // Append price to History
            //historyHolder.AddPrice(assetBid.Instrument, new Price() { Date = assetBid.Date, Ask = assetBid.Ask, Bid = assetBid.Bid });

            return Task.FromResult(0);
        }


        private async void QuoteSubscriber_MessageReceived(object sender, IInstrumentPrice e)
        {
            try { await ProcessPrice(e); }
            catch
            {
                //TODO: Log Error
                throw;
            }
        }

        public Task<string> ChangeAsync(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            throw new NotImplementedException();
        }

        public bool ValidateRequest(string userId, string pair)
        {
            throw new NotImplementedException();
        }

        public bool ValidateRequestResult(string result, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool ValidateChange(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            throw new NotImplementedException();
        }

        public bool ValidateChangeResult(string result, out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
