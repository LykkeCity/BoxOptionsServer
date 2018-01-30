using Autofac;
using BoxOptions.CoefficientCalculator.Algo;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Models;
using BoxOptions.Common.Settings;
using BoxOptions.Core.Interfaces;
using Common;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BoxOptions.CoefficientCalculator
{
    public class Calculator : ICoefficientCalculator,IStartable, IDisposable
    {   
        private bool _isSubscriberRunning;
        private readonly IAssetQuoteSubscriber _quoteSubscriber;
        private readonly IHistoryHolder _historyHolder;
        private readonly IActivityManager _activityManager;
        private readonly ILog _logger;
        private readonly CoefficientCalculatorSettings _settings;

        private readonly PriceCache _priceCache;
        private bool isDisposing = false;

        private Dictionary<string, OptionsGrid> _grids;
        private Dictionary<string, List<double>> _activities;
        

        public Calculator(CoefficientCalculatorSettings settings, IAssetQuoteSubscriber quoteSubscriber, IHistoryHolder historyHolder, IActivityManager activityManager, ILog logger)
        {            
            _isSubscriberRunning = false;
            _settings = settings;
            _quoteSubscriber = quoteSubscriber;
            _historyHolder = historyHolder;
            _activityManager = activityManager;
            _priceCache = new PriceCache();
            _logger = logger;
        }

        public void Start()
        {
            if (_historyHolder.IsStarting)
            {
                string msg = "CoefficientCalculator Waiting for History Build Up...";
                Console.WriteLine(msg);
                _logger.WriteInfoAsync("Calculator.Start", null, msg);
                _historyHolder.InitializationFinished += HistoryHolder_InitializationFinished;
            }
            else
                Task.Run(async () => await Initialize())
                    .Wait();
        }
        
        private async Task Initialize()
        {
            _grids = new Dictionary<string, OptionsGrid>();
            _activities = new Dictionary<string, List<double>>();
            Dictionary<string, DateTime> lastreport = new Dictionary<string, DateTime>();
            foreach (var instrument in _settings.Instruments)
            {
                lastreport.Add(instrument.Name, DateTime.UtcNow);
                var activity = await _activityManager.GetActivityByName(instrument.Name, instrument.ActivityFileName);
                _activities.Add(instrument.Name, activity.ActivityArray.ToList());

                var grid = new OptionsGrid(instrument.TimeToFirstOption, instrument.OptionLen, instrument.PriceSize, instrument.NPriceIndex, instrument.NTimeIndex,
                    instrument.MarginHit, instrument.MarginMiss, instrument.MaxPayoutCoeff, instrument.BookingFee, instrument.HasWeekend);

                var history = _historyHolder.GetHistory(instrument.Name);
                if (history != null && history.Length > 0)
                {

                    var currentPrice = history.Last();
                    await _logger.WriteInfoAsync("Caculator.Initialize", null, $"Current Price[{instrument.Name}] Date: {currentPrice.Date.ToString("u")}");
                    grid.InitiateGrid(_activities[instrument.Name], history.ToList(), instrument.Delta, instrument.MovingWindow, currentPrice, instrument.SmileVar);
                    _grids.Add(instrument.Name, grid);

                    Timer instrumentTimer = new Timer(instrument.Period);
                    instrumentTimer.Elapsed += (sender, args) =>
                    {
                        instrumentTimer.Stop();
                        if (isDisposing)
                            return;

                        var now = DateTime.UtcNow;
                        var newPrices = _priceCache.GetPrices(instrument.Name);

                        Price newPrice;
                        if (newPrices.Length > 0)
                        {
                            var lastPrice = newPrices.Last();
                            newPrice = new Price
                            {
                                Date = now,
                                Ask = lastPrice.Ask,
                                Bid = lastPrice.Bid
                            };
                        }
                        else
                        {
                            var lastHistoryPrice = _historyHolder.GetHistory(instrument.Name).Last();
                            newPrice = new Price
                            {
                                Date = now,
                                Ask = lastHistoryPrice.Ask,
                                Bid = lastHistoryPrice.Bid
                            };
                            //Console.WriteLine($"Current Price[{instrument.Name}] Date: {currentPrice.Date.ToString("u")}");
                        }
                        _grids[instrument.Name].UpdateCoefficients(newPrices.ToList(), newPrice, instrument.SmileVar);                        
                        if (newPrices.Length > 0 && DateTime.UtcNow > lastreport[instrument.Name].AddMinutes(30))
                        {
                            string msg = $"{DateTime.UtcNow.ToString("u")}[{instrument.Name}] Updated. New prices size:{newPrices.Length}. Current Price:{newPrice.Date.ToString("u")}";
                            Console.WriteLine(msg);
                            _logger.WriteInfoAsync("Calculator.instrumentTimer.Elapsed", null, msg);
                            lastreport[instrument.Name] = DateTime.UtcNow;
                        }

                        instrumentTimer.Start();
                    };
                    instrumentTimer.Start();
                }

            }
            StartSubscriber();
        }
        
        public void StartSubscriber()
        {   
            if (_quoteSubscriber == null)
                throw new InvalidOperationException("QuoteSubscriber not available");
                        
            _quoteSubscriber.MessageReceived += QuoteSubscriber_MessageReceived;
            _isSubscriberRunning = true;
        }
        public void StopSubscriber()
        {   
            if (_quoteSubscriber == null)
                throw new InvalidOperationException("QuoteSubscriber not available");

            _quoteSubscriber.MessageReceived -= QuoteSubscriber_MessageReceived;
            _isSubscriberRunning = false;
        }

        private Task ProcessPrice(IInstrumentPrice assetBid)
        {
            var price = assetBid.ClonePrice() as Price;
            _priceCache.AddPrice(assetBid.Instrument, price);

            return Task.FromResult(0);
        }
        private void ReinitGrid(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            var cfg = _settings.Instruments.First(s => s.Name == pair);
            var grid = new OptionsGrid(timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex, cfg.MarginHit, cfg.MarginMiss, cfg.MaxPayoutCoeff, cfg.BookingFee, cfg.HasWeekend);
            var activities = _activities[pair];
            var history = _historyHolder.GetHistory(pair);
            var currentPrice = history.Last();
            grid.InitiateGrid(activities, history.ToList(), cfg.Delta, cfg.MovingWindow, currentPrice, cfg.SmileVar);
            _grids[pair] = grid;
            string msg = $"{DateTime.UtcNow.ToString("u")}[{pair}] Updated. History size:{history.Length}. Current Price:{currentPrice.Date.ToString("u")}";
            Console.WriteLine(msg);
            _logger.WriteInfoAsync("Calculator.ReinitGrid", null, msg);
        }

        public void Dispose()
        {
            if (isDisposing)
                return;
            isDisposing = true;
            
            if (_isSubscriberRunning)
                StopSubscriber();
        }

        public Task<string> ChangeAsync(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            ReinitGrid(pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
            return Task.FromResult("OK");
        }

        public Task<string> RequestAsync(string userId, string pair)
        {
            var grid = _grids[pair].optionsGrid.ToJson();
            // Hack for old APP compatibility
            grid = grid.Replace("HitCoeff", "hitCoeff").Replace("MissCoeff", "missCoeff");
            return Task.FromResult(grid);
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

        private void QuoteSubscriber_MessageReceived(object sender, IInstrumentPrice e)
        {            
            try { Task.Run(async () => { await ProcessPrice(e); }).Wait(); }
            catch
            {
                //TODO: Log Error
                throw;
            }
        }
        private void HistoryHolder_InitializationFinished(object sender, EventArgs e)
        {
            string msg = "...CoefficientCalculator Finished Waiting for History Build Up";
            Console.WriteLine(msg);
            _logger.WriteInfoAsync("Calculator.Start", null, msg);
            _historyHolder.InitializationFinished -= HistoryHolder_InitializationFinished;
            Task.Run(async () => await Initialize())
                    .Wait();
        }

        

        
    }
}
