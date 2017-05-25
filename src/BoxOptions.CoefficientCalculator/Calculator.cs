using BoxOptions.CoefficientCalculator.Daos;
using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.CoefficientCalculator
{
    public class Calculator : ICoefficientCalculator
    {
        private bool initialized ;
        private bool isDisposing ;
        private bool isSubscriberRunning;
        private Core.Interfaces.IAssetQuoteSubscriber quoteSubscriber;
        private Core.Interfaces.IBoxOptionsHistory historyRep;

        // TODO: configuration
        private int numDaysHistory;

        private HistoryHolder historyHolder;

        private List<InstrumentPrice> lastPricesCache;


        public Calculator()
        {
            initialized = false;
            isDisposing = false;
            isSubscriberRunning = false;
            numDaysHistory = 7;

            historyHolder = new HistoryHolder();
            lastPricesCache = new List<InstrumentPrice>();
        }

        public void Init(Core.Interfaces.IAssetQuoteSubscriber quoteSubscriber, Core.Interfaces.IBoxOptionsHistory historyRep)
        {
            this.quoteSubscriber = quoteSubscriber;
            this.historyRep = historyRep;

            // TODO: Load history to Holder()

            //historyHolder.BuildHistory(historyRep.GetAssetHistory(DateTime.Today.AddDays(-numDaysHistory), DateTime.UtcNow));



            initialized = true;
        }

        public void StartSubscriber()
        {
            if (!initialized )
                throw new InvalidOperationException("Calculator not initialized");
            if (quoteSubscriber == null)
                throw new InvalidOperationException("QuoteSubscriber not available");
                        
            quoteSubscriber.MessageReceived += QuoteSubscriber_MessageReceived;
            isSubscriberRunning = true;
        }        
        public void StopSubscriber()
        {
            if (!initialized)
                throw new InvalidOperationException("Calculator not initialized");
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
            initialized = false;

            if (isSubscriberRunning)
                StopSubscriber();
            quoteSubscriber = null;


        }

        private Task ProcessPrice(InstrumentPrice assetBid)
        {
            // Append price to History
            historyHolder.AddPrice(assetBid.Instrument, new Price() { Date = assetBid.Date, Ask = assetBid.Ask, Bid = assetBid.Bid });

            return Task.FromResult(0);
        }


        private async void QuoteSubscriber_MessageReceived(object sender, InstrumentPrice e)
        {
            try { await ProcessPrice(e); }
            catch
            {
                //TODO: Log Error
                throw;
            }
        }
    }
}
