﻿using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    
    public class AssetQuoteSubscriber : IAssetQuoteSubscriber
    {
        /// <summary>
        /// Settings Object
        /// </summary>
        private readonly BoxOptionsSettings settings;
        /// <summary>
        /// Incoming Asset Cache
        /// </summary>
        private readonly List<AssetPairBid> assetCache;

        /// <summary>
        /// RabbitMQ Subscriber
        /// </summary>
        private RabbitMqSubscriber<AssetQuote> subscriber;

        /// <summary>
        /// Logger Object
        /// </summary>
        ILog log;

        /// <summary>
        /// Time stamp of last received message;
        /// </summary>
        DateTime lastMessageTimeStamp;
        
        /// <summary>
        /// Connection data check timer.
        /// </summary>
        System.Threading.Timer checkConnectionTimer;
        
        bool isDisposing;
        
        /// <summary>
        /// Thrown when a new message is received from RabbitMQ Queue
        /// </summary>
        public event EventHandler<AssetPairBid> MessageReceived;

        public AssetQuoteSubscriber(BoxOptionsSettings settings, ILog log)
        {
            isDisposing = false;
            lastMessageTimeStamp = DateTime.UtcNow;
            assetCache = new List<AssetPairBid>();
            this.settings = settings;
            this.log = log;
            checkConnectionTimer = new System.Threading.Timer(CheckConnectionTimerCallback, null, -1, -1);
            
        }

        public void Start()
        {
            subscriber = new RabbitMqSubscriber<AssetQuote>(new RabbitMqSubscriberSettings
            {
                ConnectionString = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOConnectionString,
                ExchangeName = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOExchangeName,
                QueueName = settings.BoxOptionsApi.PricesSettingsBoxOptions.RabbitMqBOPricesQueueName,
                IsDurable = false
            })
               .SetMessageDeserializer(new MessageDeserializer<AssetQuote>())
               .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
               .SetLogger(log)
               .Subscribe(ProcessMessage)
               .Start();

            // Start Timer to check incoming dataconnection
            checkConnectionTimer.Change(settings.BoxOptionsApi.PricesSettingsBoxOptions.IncomingDataCheckInterval * 1000, -1);

        }
        public void Dispose()
        {
            isDisposing = true;

            subscriber.Stop();
            checkConnectionTimer.Change(-1, -1);
            checkConnectionTimer.Dispose();

        }

        private Task ProcessMessage(AssetQuote assetQuote)
        {
            //Message received, update timestamp.
            lastMessageTimeStamp = DateTime.UtcNow;

            // TODO: update or clear asset filtering
            // Filter Asset 
            if (!Common.AllowedAssets.Contains(assetQuote.AssetPair))
            {
                // Not in allowed assets list, discard entry
                return Task.FromResult(0);
            }
            else
            {
                // Asset allowed, add it to cache and
                // invoke MessageReceived event


                // Get Asset from cache
                AssetPairBid assetbid = (from a in assetCache
                                         where a.Id == assetQuote.AssetPair
                                         select a).FirstOrDefault();
                if (assetbid == null)
                {
                    // AssetPair is not in cache
                    // Add AssetQuote to cache
                    assetbid = new AssetPairBid()
                    {
                        Id = assetQuote.AssetPair,
                        Date = assetQuote.Timestamp,
                        Ask = assetQuote.IsBuy == Common.ASK ? assetQuote.Price : 0,
                        Bid = assetQuote.IsBuy == Common.ASK ? 0 : assetQuote.Price
                    };
                    assetCache.Add(assetbid);
                }
                else
                {
                    // AssetPair is in cache
                    // Update Bid Quote
                    if (assetQuote.IsBuy == Common.ASK)
                        assetbid.Ask = assetQuote.Price;
                    else
                        assetbid.Bid = assetQuote.Price;
                }

                // TODO: clear date override
                // override asset bid with server UTC date.now
                assetbid.Date = DateTime.UtcNow;

                MessageReceived?.Invoke(this, assetbid);
                return Task.FromResult(0);
            }
        }

        private void CheckConnectionTimerCallback(object status)
       {

            // Stop Timer
            checkConnectionTimer.Change(-1, -1);

            DateTime currentdate = DateTime.UtcNow;
            double SecondsSinceLastMessage = (currentdate - lastMessageTimeStamp).TotalSeconds;
            Console.WriteLine("SecondsSinceLastMessage: {0:F2}", SecondsSinceLastMessage);

            // Last message receive longer than allowed in IncomingDataCheckInterval
            if (SecondsSinceLastMessage > settings.BoxOptionsApi.PricesSettingsBoxOptions.IncomingDataCheckInterval)
            {
                //Check if current date is in exclusion interval (feeds are not available)
                bool InExclusionInterval = CheckExclusionInterval(currentdate);
                if (!InExclusionInterval)
                {
                    // Not in exclusion interval, report error.
                    string logmessage = string.Format("BoxOptions Server: No Messages from RabbitMQ for {0}", currentdate - lastMessageTimeStamp);                    
                    log.WriteWarningAsync("AssetQuoteSubscriber", "CheckConnectionTimerCallback", "", logmessage);                    
                }
            }
            

            
            
            // Re-Start timer if not disposing.
            if (!isDisposing)
                checkConnectionTimer.Change(settings.BoxOptionsApi.PricesSettingsBoxOptions.IncomingDataCheckInterval * 1000, -1);

        }

        private bool CheckExclusionInterval(DateTime utcNow)
        {
            try
            {
                string[] startval = settings.BoxOptionsApi.PricesSettingsBoxOptions.PricesWeekExclusionStart.Split(';');
                string[] starthour = startval[1].Split(':');
                string[] endval = settings.BoxOptionsApi.PricesSettingsBoxOptions.PricesWeekExclusionEnd.Split(';');
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
                log.WriteErrorAsync("AssetQuoteSubscriber", "CheckExclusionInterval", "", ex);
                return true;
            }
        }
    }
}