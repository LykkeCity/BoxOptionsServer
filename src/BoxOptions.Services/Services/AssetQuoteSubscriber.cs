using Autofac;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Models;
using BoxOptions.Common.RabbitMq;
using BoxOptions.Common.Settings;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Services
{

    public class AssetQuoteSubscriber : IAssetQuoteSubscriber, IStartable, IDisposable
    {
        private static readonly object AssetConfigurationLock = new object();

        /// <summary>
        /// Settings Object
        /// </summary>
        private readonly BoxOptionsApiSettings _settings;
        /// <summary>
        /// Incoming Asset Cache
        /// </summary>
        private readonly List<InstrumentPrice> _assetCache;
        /// <summary>
        /// Logger Object
        /// </summary>
        private readonly ILog _logger;

        private readonly IAssetDatabase _history;
        private readonly IBoxConfigRepository _boxRepo;

        /// <summary>
        /// RabbitMQ Subscriber
        /// </summary>
        private RabbitMqSubscriber<BestBidAsk> _primarySubscriber;
        private RabbitMqSubscriber<AssetQuote> _secondarySubscriber;

        private Dictionary<string, InstrumentPrice> _lastPrices;

        private List<string> _primaryGameInstruments = null;
        private List<string> _secondaryGameInstruments = null;

        /// <summary>
        /// Time stamp of last received message from Primary Stream
        /// </summary>
        private DateTime _primaryStreamLastMessageTimeStamp;
        /// <summary>
        /// Time stamp of last received message from Secondary Stream
        /// </summary>
        private DateTime _secondaryStreamLastMessageTimeStamp;

        /// <summary>
        /// Connection data check timer.
        /// </summary>
        private System.Threading.Timer _checkConnectionTimer;
        private bool _isDisposing;

        private DateTime _lastErrorDate = DateTime.MinValue;
        private string _lastErrorMessage = "";
        private IBoxSize[] _assetConfiguration;

        /// <summary>
        /// Thrown when a new message is received from RabbitMQ Queue
        /// </summary>
        public event EventHandler<IInstrumentPrice> MessageReceived;

        public AssetQuoteSubscriber(BoxOptionsApiSettings settings, ILog log, IAssetDatabase history, IBoxConfigRepository boxRepo)
        {
            _isDisposing = false;
            _primaryStreamLastMessageTimeStamp = _secondaryStreamLastMessageTimeStamp = DateTime.UtcNow;
            _assetCache = new List<InstrumentPrice>();
            this._settings = settings;
            this._logger = log;
            this._history = history;
            this._boxRepo = boxRepo;
            _lastPrices = new Dictionary<string, InstrumentPrice>();
            _checkConnectionTimer = new System.Threading.Timer(CheckConnectionTimerCallback, null, -1, -1);

            _primaryGameInstruments = settings.PricesSettingsBoxOptions.PrimaryFeed.AllowedAssets.ToList();
            if (settings.PricesSettingsBoxOptions.SecondaryFeed != null)
                _secondaryGameInstruments = settings.PricesSettingsBoxOptions.SecondaryFeed.AllowedAssets.ToList();
        }

        public async void Start()
        {
            var boxes = await _boxRepo.GetAll();
            // On start up call AssetConfigurationLock is not needed.
            _assetConfiguration = boxes.ToArray();

            // Start Primary Subscriber. Uses BestBidAsk Model
            _primarySubscriber = CreateSubscriber<BestBidAsk>(_settings.PricesSettingsBoxOptions.PrimaryFeed,
                PrimaryMessageReceived_BestBidAsk);

            LogInfo("Start", $"AssetQuoteSubscriber Primary Feed [{_settings.PricesSettingsBoxOptions.PrimaryFeed.RabbitMqConnectionString}]");
            if (_settings.PricesSettingsBoxOptions.SecondaryFeed != null)
            {
                // Start Secondary Subscriber. Uses Asset quote model
                _secondarySubscriber = CreateSubscriber<AssetQuote>(
                    _settings.PricesSettingsBoxOptions.SecondaryFeed,
                    SecondaryMessageReceived_AssetQuote);
                if (_secondarySubscriber != null)
                    LogInfo("Start", $"AssetQuoteSubscriber Secondary Feed [{_settings.PricesSettingsBoxOptions.SecondaryFeed.RabbitMqConnectionString}]");
            }

            // Start Timer to check incoming dataconnection
            int CheckInterval = _settings.PricesSettingsBoxOptions.NoFeedSlackReportInSeconds;
            _checkConnectionTimer.Change(CheckInterval * 1000, -1);
        }
        public void Dispose()
        {
            _isDisposing = true;

            if (_primarySubscriber != null)
                _primarySubscriber.Stop();
            if (_secondarySubscriber != null)
                _secondarySubscriber.Stop();

            _checkConnectionTimer.Change(-1, -1);
            _checkConnectionTimer.Dispose();

        }

        private async void LogInfo(string process, string info)
        {
            await _logger?.WriteInfoAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, info, DateTime.UtcNow);
        }
        private async void LogWarning(string process, string warning)
        {
            await _logger?.WriteWarningAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, warning, DateTime.UtcNow);

        }
        private async void LogError(string process, Exception ex)
        {
            Exception innerEx;
            if (ex.InnerException != null)
                innerEx = ex.InnerException;
            else
                innerEx = ex;

            bool LogError;
            if (_lastErrorMessage != innerEx.Message)
            {
                LogError = true;
            }
            else
            {
                if (DateTime.UtcNow > _lastErrorDate.AddMinutes(1))
                    LogError = true;
                else
                    LogError = false;
            }


            if (LogError)
            {
                _lastErrorMessage = innerEx.Message;
                _lastErrorDate = DateTime.UtcNow;
                await _logger?.WriteErrorAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, innerEx);
                //Console.WriteLine("Logged: {0}", innerEx.Message);
            }
        }

        
        private RabbitMqSubscriber<TMessage> CreateSubscriber<TMessage>(FeedSettings settings, Func<TMessage, Task> handler)
        {
            if (settings.RabbitMqConnectionString == null || settings.RabbitMqConnectionString == "" || settings.RabbitMqConnectionString == "null")
                return null;

            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.RabbitMqConnectionString,
                ExchangeName = settings.RabbitMqExchangeName,
                QueueName = settings.RabbitMqQueueName,
                IsDurable = false
            };

            var subscriber = new RabbitMqSubscriber<TMessage>(subscriptionSettings,
                new DefaultErrorHandlingStrategy(_logger, subscriptionSettings))
                .SetMessageDeserializer(new ErrorLoggingJsonMessageDeserializer<TMessage>(_logger))
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(_logger)
                .Subscribe(handler)
                .Start();

            return subscriber;
        }        

        #region Process Subscriber Incoming Message
        private Task PrimaryMessageReceived_BestBidAsk(BestBidAsk bestBidAsk)
        {
            //Message received, update timestamp.
            _primaryStreamLastMessageTimeStamp = DateTime.UtcNow;

            //Console.WriteLine(bestBidAsk);

            // Filter Asset from Primary Stream Configuration File
            if (!_primaryGameInstruments.Contains(bestBidAsk.Asset))
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            else
                return ProcessBestBidAsk(bestBidAsk);
        }
        private Task PrimaryMessageReceived_AssetQuote(AssetQuote assetQuote)
        {
            //Message received, update timestamp.
            _primaryStreamLastMessageTimeStamp = DateTime.UtcNow;

            // Filter Asset from Primary Stream Configuration File
            if (!_primaryGameInstruments.Contains(assetQuote.AssetPair))
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            else
                return ProcessAssetQuote(assetQuote);
        }
        private Task SecondaryMessageReceived_AssetQuote(AssetQuote assetQuote)
        {
            //Message received, update timestamp.
            _secondaryStreamLastMessageTimeStamp = DateTime.UtcNow;

            // Filter Asset from Primary Stream Configuration File
            if (!_secondaryGameInstruments.Contains(assetQuote.AssetPair))
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            else
                return ProcessAssetQuote(assetQuote);
        }
        #endregion

        private Task ProcessBestBidAsk(BestBidAsk bestBidAsk)
        {

            // If Price is zero/null publish exception to support slack channel
            // and discard entry
            if (bestBidAsk.BestAsk == null || bestBidAsk.BestAsk <= 0
                || bestBidAsk.BestBid == null || bestBidAsk.BestBid <= 0)
            {
                //log                
                LogWarning("ProcessBestBidAsk", string.Format("Received BestBidAsk with price zero [0], BestBidAsk discarded. {0}", bestBidAsk));

                // Discard it
                return Task.FromResult(0);
            }
            
            InstrumentPrice assetbid = new InstrumentPrice()
            {
                Instrument = bestBidAsk.Asset,
                Source = bestBidAsk.Source,
                Ask = bestBidAsk.BestAsk.Value,
                Bid = bestBidAsk.BestBid.Value,
                Date = bestBidAsk.Timestamp,
                ReceiveDate = DateTime.UtcNow
            };

            OnMessageReceived(assetbid);

            return Task.FromResult(0);

        }
        private Task ProcessAssetQuote(AssetQuote assetQuote)
        {

            // If Price is zero publish exception to support slack channel
            if (assetQuote.Price <= 0)
            {
                LogWarning("ProcessAssetQuote", string.Format("Received AssetQuote with price zero [0], AssetQuote discarded. {0}", assetQuote));
                return Task.FromResult(0);
            }


            // Get Asset from cache
            InstrumentPrice assetbid = (from a in _assetCache
                                        where a.Instrument == assetQuote.AssetPair
                                        select a).FirstOrDefault();
            if (assetbid == null)
            {
                // AssetPair is not in cache
                // Add AssetQuote to cache
                assetbid = new InstrumentPrice()
                {
                    Instrument = assetQuote.AssetPair,
                    Source = "AssetQuote",
                    Date = assetQuote.Timestamp,
                    Ask = assetQuote.IsBuy == Statics.ASK ? assetQuote.Price : 0,
                    Bid = assetQuote.IsBuy == Statics.ASK ? 0 : assetQuote.Price,
                    ReceiveDate = DateTime.UtcNow
                };
                _assetCache.Add(assetbid);
            }
            else
            {
                // AssetPair is in cache
                // Update Bid Quote
                if (assetQuote.IsBuy == Statics.ASK)
                    assetbid.Ask = assetQuote.Price;
                else
                    assetbid.Bid = assetQuote.Price;
            }
            
            // Only publish if bid and ask prices have changed since last publish
            bool publish = false;
            if (!_lastPrices.ContainsKey(assetbid.Instrument))
            {
                // Asset not in history, add it and set publish = true
                _lastPrices.Add(assetQuote.AssetPair, (InstrumentPrice)assetbid.ClonePrice());
                publish = true;
            }
            else
            {
                if (_lastPrices[assetbid.Instrument].Ask > 0 &&
                    _lastPrices[assetbid.Instrument].Bid > 0 &&
                    (_lastPrices[assetbid.Instrument].Ask == assetbid.Ask || _lastPrices[assetbid.Instrument].Bid == assetbid.Bid))
                {
                    // One price (Ask or Bid) has not changed. do not publish it
                    // Must only be published when both Ask Bid prices have changed                    
                    publish = false;
                }
                else
                {
                    // Both prices have changed publish it and assign this new bid to history
                    publish = true;
                    _lastPrices[assetbid.Instrument] = (InstrumentPrice)assetbid.ClonePrice();
                }
            }

            if (assetbid.Ask <= 0 || assetbid.Bid <= 0)
            {
                publish = false;
            }

            if (publish)
            {
                OnMessageReceived(assetbid);
            }
            return Task.FromResult(0);

        }

        private void OnMessageReceived(InstrumentPrice bestBidAsk)
        {
            IBoxSize assetCfg = null;

            // Lock Asset Configuration
            lock (AssetConfigurationLock)
            {
                assetCfg = _assetConfiguration.FirstOrDefault(a => a.AssetPair == bestBidAsk.Instrument);
            }
            if (assetCfg == null)
                return;

            // If asset configured to save history, add it to history
            if (assetCfg.SaveHistory)
            {
                _history?.AddToAssetHistory(new BestBidAsk()
                {
                    Asset = bestBidAsk.Instrument,
                    BestAsk = bestBidAsk.Ask,
                    BestBid = bestBidAsk.Bid,
                    Source = bestBidAsk.Source,
                    Timestamp = bestBidAsk.Date,
                    ReceiveDate = DateTime.UtcNow
                });
            }

            // If asset allowed in game, raise event
            if (assetCfg.GameAllowed)
                MessageReceived?.Invoke(this, (InstrumentPrice)bestBidAsk.ClonePrice());
        }

        private void CheckConnectionTimerCallback(object status)
        {
            DateTime currentdate = DateTime.UtcNow;
#if DEBUG
            Console.WriteLine("{0} > CheckConnectionTimerCallback", currentdate.ToString("HH:mm:ss.fff"));
#endif
            // Stop Timer
            _checkConnectionTimer.Change(-1, -1);

            // Check Primary Stream
            #region Primary
            if (_primarySubscriber != null)
            {
                double PrimaryStreamLastMessage = (currentdate - _primaryStreamLastMessageTimeStamp).TotalSeconds;

                // Last message receive longer than allowed in IncomingDataCheckInterval
                if (PrimaryStreamLastMessage > _settings.PricesSettingsBoxOptions.PrimaryFeed.IncomingDataCheckInterval)
                {
                    //Check if current date is in exclusion interval (feeds are not available)
                    bool InExclusionInterval = CheckExclusionInterval(currentdate,
                        _settings.PricesSettingsBoxOptions.PrimaryFeed.PricesWeekExclusionStart,
                        _settings.PricesSettingsBoxOptions.PrimaryFeed.PricesWeekExclusionEnd);
                    if (!InExclusionInterval)
                    {
                        // Not in exclusion interval, report error.        
                        string msg = string.Format("No Messages from Primary Feed for {0}", currentdate - _primaryStreamLastMessageTimeStamp);
                        Console.WriteLine("{0} > {1}", currentdate.ToString("HH:mm:ss.fff"), msg);
                        LogWarning("CheckConnectionTimerCallback", msg);
                    }
                }
            }
            #endregion

            // Check Secondary Stream
            #region Secondary
            if (_secondarySubscriber != null)
            {
                double SecondaryStreamLastMessage = (currentdate - _secondaryStreamLastMessageTimeStamp).TotalSeconds;

                // Last message receive longer than allowed in IncomingDataCheckInterval
                if (SecondaryStreamLastMessage > _settings.PricesSettingsBoxOptions.SecondaryFeed.IncomingDataCheckInterval)
                {
                    //Check if current date is in exclusion interval (feeds are not available)
                    bool InExclusionInterval = CheckExclusionInterval(currentdate,
                        _settings.PricesSettingsBoxOptions.SecondaryFeed.PricesWeekExclusionStart,
                        _settings.PricesSettingsBoxOptions.SecondaryFeed.PricesWeekExclusionEnd);
                    if (!InExclusionInterval)
                    {
                        // Not in exclusion interval, report error.                                                                    
                        string msg = string.Format("No Messages from Secondary Feed for {0}", currentdate - _secondaryStreamLastMessageTimeStamp);
                        Console.WriteLine("{0} > {1}", currentdate.ToString("HH:mm:ss.fff"), msg);
                        LogWarning("CheckConnectionTimerCallback", msg);
                    }
                }
            }
            #endregion

            // Re-start timer if not disposing.
            if (!_isDisposing)
                _checkConnectionTimer.Change(_settings.PricesSettingsBoxOptions.NoFeedSlackReportInSeconds * 1000, -1);

        }
        private bool CheckExclusionInterval(DateTime utcNow, string WeekExclusionStart, string WeekExclusionEnd)
        {
            try
            {
                string[] startval = WeekExclusionStart.Split(';');
                string[] starthour = startval[1].Split(':');
                string[] endval = WeekExclusionEnd.Split(';');
                string[] endhour = endval[1].Split(':');

                DayOfWeek StartDayofWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), startval[0]);
                DayOfWeek EndDayofWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), endval[0]);

                int sHour = int.Parse(starthour[0]);
                int sMinute = int.Parse(starthour[1]);
                int sSecond = int.Parse(starthour[2]);

                int eHour = int.Parse(endhour[0]);
                int eMinute = int.Parse(endhour[1]);
                int eSecond = int.Parse(endhour[2]);

                DateTime startdate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, sHour, sMinute, sSecond);
                while (startdate.DayOfWeek != StartDayofWeek)
                {
                    startdate = startdate.AddDays(-1);
                }

                DateTime enddate = new DateTime(startdate.Year, startdate.Month, startdate.Day, eHour, eMinute, eSecond);
                while (enddate.DayOfWeek != EndDayofWeek)
                {
                    enddate = enddate.AddDays(1);
                }

                if (utcNow > startdate &&
                    utcNow < enddate)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogError("CheckExclusionInterval", ex);
                return true;
            }
        }

        public async Task<bool> ReloadAssetConfiguration()
        {
            try
            {
                var boxes = await _boxRepo.GetAll();
                // Lock Asset Configuration
                lock (AssetConfigurationLock)
                {
                    _assetConfiguration = boxes.ToArray();
                }
                LogInfo("ReloadAssetConfiguration", "Reloaded asset configuration from database");
                return true;
            }
            catch (Exception ex01)
            {
                LogError("ReloadAssetConfiguration", ex01);
                return false;
            }
        }
    }
}
