using Autofac;
using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using BoxOptions.Core;
using BoxOptions.Core.Models;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Services
{

    public class AssetQuoteSubscriber : IAssetQuoteSubscriber, IStartable, IDisposable
    {
        /// <summary>
        /// Settings Object
        /// </summary>
        private readonly BoxOptionsSettings settings;
        /// <summary>
        /// Incoming Asset Cache
        /// </summary>
        private readonly List<InstrumentPrice> assetCache;

        /// <summary>
        /// RabbitMQ Subscriber
        /// </summary>

        private RabbitMqSubscriber<BestBidAsk> primarySubscriber;
        private RabbitMqSubscriber<AssetQuote> secondarySubscriber;


        Dictionary<string, InstrumentPrice> lastPrices;

        List<string> PrimaryGameInstruments = null;
        List<string> SecondaryGameInstruments = null;

        /// <summary>
        /// Logger Object
        /// </summary>
        ILog appLog;

        /// <summary>
        /// Time stamp of last received message from Primary Stream
        /// </summary>
        DateTime primaryStreamLastMessageTimeStamp;

        /// <summary>
        /// Time stamp of last received message from Secondary Stream
        /// </summary>
        DateTime secondaryStreamLastMessageTimeStamp;

        /// <summary>
        /// Connection data check timer.
        /// </summary>
        System.Threading.Timer checkConnectionTimer;
        
        bool isDisposing;

        IAssetDatabase history;

        DateTime LastErrorDate = DateTime.MinValue;
        string LastErrorMessage = "";

        /// <summary>
        /// Thrown when a new message is received from RabbitMQ Queue
        /// </summary>
        public event EventHandler<InstrumentPrice> MessageReceived;

        public AssetQuoteSubscriber(BoxOptionsSettings settings, ILog log, IAssetDatabase history)
        {
            isDisposing = false;
            primaryStreamLastMessageTimeStamp = secondaryStreamLastMessageTimeStamp = DateTime.UtcNow;            
            assetCache = new List<InstrumentPrice>();
            this.settings = settings;
            this.appLog = log;
            this.history = history;
            lastPrices = new Dictionary<string, InstrumentPrice>();
            checkConnectionTimer = new System.Threading.Timer(CheckConnectionTimerCallback, null, -1, -1);

            PrimaryGameInstruments = settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.AllowedAssets.ToList();
            if (settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed != null)
                SecondaryGameInstruments = settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.AllowedAssets.ToList();
        }

        public void Start()
        {
            // Start Primary Subscriber. Uses BestBidAsk Model
            primarySubscriber = CreateBestBidSubscriber(settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed,
                PrimaryMessageReceived_BestBidAsk);            
            
            LogInfo("Start", $"AssetQuoteSubscriber Primary Feed [{settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.RabbitMqConnectionString}]");
            if (settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed != null)
            {
                // Start Secondary Subscriber. Uses Asset quote model
                secondarySubscriber = CreateAssetSubscriber(
                    settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed,
                    SecondaryMessageReceived_AssetQuote);
                if (secondarySubscriber != null)
                    LogInfo("Start", $"AssetQuoteSubscriber Secondary Feed [{settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.RabbitMqConnectionString}]");                    
            }

            // Start Timer to check incoming dataconnection (checks every 30 seconds)
            int CheckInterval = settings.BoxOptionsApi.PricesSettingsBoxOptions.NoFeedSlackReportInSeconds;
            checkConnectionTimer.Change(CheckInterval * 1000, -1);
        }
        public void Dispose()
        {
            isDisposing = true;

            if (primarySubscriber != null)
                primarySubscriber.Stop();
            if (secondarySubscriber != null)
                secondarySubscriber.Stop();

            checkConnectionTimer.Change(-1, -1);
            checkConnectionTimer.Dispose();

        }

        private async void LogInfo(string process, string info)
        {
            await appLog?.WriteInfoAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, info, DateTime.UtcNow);
        }
        private async void LogWarning(string process, string warning)
        {
            await appLog?.WriteWarningAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, warning, DateTime.UtcNow);

        }
        private async void LogError(string process, Exception ex)
        {
            Exception innerEx;
            if (ex.InnerException != null)
                innerEx = ex.InnerException;
            else
                innerEx = ex;

            bool LogError;
            if (LastErrorMessage != innerEx.Message)
            {
                LogError = true;
            }
            else
            {
                if (DateTime.UtcNow > LastErrorDate.AddMinutes(1))
                    LogError = true;
                else
                    LogError = false;
            }


            if (LogError)
            {
                LastErrorMessage = innerEx.Message;
                LastErrorDate = DateTime.UtcNow;
                await appLog?.WriteErrorAsync("BoxOptions.Services.AssetQuoteSubscriber", process, null, innerEx);
                //Console.WriteLine("Logged: {0}", innerEx.Message);
            }
        }

        #region Create Subscribers
        private RabbitMqSubscriber<AssetQuote> CreateAssetSubscriber(FeedSettings settings, Func<AssetQuote, Task> callback)
        {
            if (settings.RabbitMqConnectionString == null || settings.RabbitMqConnectionString == "" || settings.RabbitMqConnectionString == "null")
                return null;

            RabbitMqSubscriber<AssetQuote> subscriber = new RabbitMqSubscriber<AssetQuote>(new RabbitMqSubscriberSettings
            {
                ConnectionString = settings.RabbitMqConnectionString,
                ExchangeName = settings.RabbitMqExchangeName,
                QueueName = settings.RabbitMqQueueName,
                IsDurable = false
            })
              .SetMessageDeserializer(new MessageDeserializer<AssetQuote>())
              .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
              .SetLogger(appLog)
              .Subscribe(callback)
              .Start();

            return subscriber;
        }
        private RabbitMqSubscriber<BestBidAsk> CreateBestBidSubscriber(FeedSettings settings, Func<BestBidAsk, Task> callback)
        {
            if (settings.RabbitMqConnectionString == null || settings.RabbitMqConnectionString == "" || settings.RabbitMqConnectionString == "null")
                return null;

            RabbitMqSubscriber<BestBidAsk> subscriber = new RabbitMqSubscriber<BestBidAsk>(new RabbitMqSubscriberSettings
            {
                ConnectionString = settings.RabbitMqConnectionString,
                ExchangeName = settings.RabbitMqExchangeName,
                QueueName = settings.RabbitMqQueueName,
                IsDurable = false
            })
              .SetMessageDeserializer(new MessageDeserializer<BestBidAsk>())
              .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
              .SetLogger(appLog)
              .Subscribe(callback)
              .Start();

            return subscriber;
        }
        #endregion

        #region Process Subscriber Incoming Message
        private Task PrimaryMessageReceived_BestBidAsk(BestBidAsk bestBidAsk)
        {
            //Message received, update timestamp.
            primaryStreamLastMessageTimeStamp = DateTime.UtcNow;

            //Console.WriteLine(bestBidAsk);
            
            // Filter Asset from Primary Stream Configuration File
            if (!PrimaryGameInstruments.Contains(bestBidAsk.Asset))
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            else
                return ProcessBestBidAsk(bestBidAsk);
        }
        private Task PrimaryMessageReceived_AssetQuote(AssetQuote assetQuote)        
        {
            //Message received, update timestamp.
            primaryStreamLastMessageTimeStamp = DateTime.UtcNow;
            
            // Filter Asset from Primary Stream Configuration File
            if (!PrimaryGameInstruments.Contains(assetQuote.AssetPair))
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            else
                return ProcessAssetQuote(assetQuote);
        }
        private Task SecondaryMessageReceived_AssetQuote(AssetQuote assetQuote)
        {
            //Message received, update timestamp.
            secondaryStreamLastMessageTimeStamp = DateTime.UtcNow;
            
            // Filter Asset from Primary Stream Configuration File
            if (!SecondaryGameInstruments.Contains(assetQuote.AssetPair))
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
                || bestBidAsk.BestBid == null ||bestBidAsk.BestBid<= 0)
            {
                //log                
                LogWarning("ProcessBestBidAsk", string.Format("Received BestBidAsk with price zero [0], BestBidAsk discarded. {0}", bestBidAsk));                
                
                // Discard it
                return Task.FromResult(0);
            }
            
            // TODO: clear date override
            InstrumentPrice assetbid = new InstrumentPrice()
            {
                Instrument = bestBidAsk.Asset,
                Source = bestBidAsk.Source,
                Ask = bestBidAsk.BestAsk.Value,
                Bid = bestBidAsk.BestBid.Value,
                Date = DateTime.UtcNow
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
            InstrumentPrice assetbid = (from a in assetCache
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
                    Bid = assetQuote.IsBuy == Statics.ASK ? 0 : assetQuote.Price                    
                };
                assetCache.Add(assetbid);
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

            // TODO: clear date override
            // override asset bid with server UTC date.now
            assetbid.Date = DateTime.UtcNow;

            // Only publish if bid and ask prices have changed since last publish
            bool publish = false;
            if (!lastPrices.ContainsKey(assetbid.Instrument))
            {
                // Asset not in history, add it and set publish = true
                lastPrices.Add(assetQuote.AssetPair, (InstrumentPrice)assetbid.ClonePrice());
                publish = true;
            }
            else
            {
                if (lastPrices[assetbid.Instrument].Ask > 0 &&
                    lastPrices[assetbid.Instrument].Bid > 0 &&
                    (lastPrices[assetbid.Instrument].Ask == assetbid.Ask || lastPrices[assetbid.Instrument].Bid == assetbid.Bid))
                {
                    // One price (Ask or Bid) has not changed. do not publish it
                    // Must only be published when both Ask Bid prices have changed                    
                    publish = false;
                }
                else
                {
                    // Both prices have changed publish it and assign this new bid to history
                    publish = true;
                    lastPrices[assetbid.Instrument] = (InstrumentPrice)assetbid.ClonePrice();
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
            // Add to Asset History
            history?.AddToAssetHistory(new BestBidAsk()
            {
                Asset = bestBidAsk.Instrument,
                BestAsk = bestBidAsk.Ask,
                BestBid = bestBidAsk.Bid,
                Source = bestBidAsk.Source,
                Timestamp = bestBidAsk.Date
            });
            MessageReceived?.Invoke(this, (InstrumentPrice)bestBidAsk.ClonePrice());
        }

        private void CheckConnectionTimerCallback(object status)
       {
            DateTime currentdate = DateTime.UtcNow;

            // Stop Timer
            checkConnectionTimer.Change(-1, -1);

            // Check Primary Stream
            #region Primary
            if (primarySubscriber != null)
            {
                double PrimaryStreamLastMessage = (currentdate - primaryStreamLastMessageTimeStamp).TotalSeconds;
                
                // Last message receive longer than allowed in IncomingDataCheckInterval
                if (PrimaryStreamLastMessage > settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.IncomingDataCheckInterval)
                {
                    //Check if current date is in exclusion interval (feeds are not available)
                    bool InExclusionInterval = CheckExclusionInterval(currentdate,
                        settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.PricesWeekExclusionStart,
                        settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.PricesWeekExclusionEnd);
                    if (!InExclusionInterval)
                    {
                        // Not in exclusion interval, report error.                                            
                        LogWarning("CheckConnectionTimerCallback", string.Format("No Messages from Primary Feed for {0}", currentdate - primaryStreamLastMessageTimeStamp));
                    }
                }
            }
            #endregion

            // Check Secondary Stream
            #region Secondary
            if (secondarySubscriber != null)
            {
                double SecondaryStreamLastMessage = (currentdate - secondaryStreamLastMessageTimeStamp).TotalSeconds;

                // Last message receive longer than allowed in IncomingDataCheckInterval
                if (SecondaryStreamLastMessage > settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.IncomingDataCheckInterval)
                {
                    //Check if current date is in exclusion interval (feeds are not available)
                    bool InExclusionInterval = CheckExclusionInterval(currentdate,
                        settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.PricesWeekExclusionStart,
                        settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.PricesWeekExclusionEnd);
                    if (!InExclusionInterval)
                    {
                        // Not in exclusion interval, report error.                                                                    
                        LogWarning("CheckConnectionTimerCallback", string.Format("No Messages from Secondary Feed for {0}", currentdate - primaryStreamLastMessageTimeStamp));
                    }
                }
            }
            #endregion

            // Re-start timer if not disposing.
            if (!isDisposing)
                checkConnectionTimer.Change(settings.BoxOptionsApi.PricesSettingsBoxOptions.NoFeedSlackReportInSeconds * 1000, -1);

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
    }
}
