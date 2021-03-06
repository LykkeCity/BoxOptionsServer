﻿using Autofac;
using BoxOptions.Common.Extensions;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Models;
using BoxOptions.Common.Settings;
using BoxOptions.Core.Interfaces;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxOptions.Services
{
    public class HistoryHolder : IHistoryHolder, IStartable, IDisposable
    {
        public event EventHandler InitializationFinished;

        private static object HistoryLock = new object();

        private readonly BoxOptionsApiSettings _settings;
        private readonly Dictionary<string, LinkedList<Price>> _holder;
        private readonly IAssetQuoteSubscriber _subscriber;
        private readonly IAssetDatabase _assetDatabase;
        private readonly ILog _log;

        private string[] historyAssets;

        bool isStarting;

        public HistoryHolder(BoxOptionsApiSettings settings, IAssetQuoteSubscriber subscriber, IAssetDatabase assetDatabase, ILog appLog)
        {
            _settings = settings;
            _assetDatabase = assetDatabase;
            _log = appLog;
            _subscriber = subscriber;
            _holder = new Dictionary<string, LinkedList<Price>>();
            isStarting = true;
        }

        public bool IsStarting { get => isStarting; }

        public async void Start()
        {
            Console.WriteLine("{0} | History Holder Starting", DateTime.UtcNow.ToTimeString());
            await _log?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, "History Holder Started", DateTime.UtcNow);

            string StartLog = string.Format("{0} | Getting History", DateTime.UtcNow.ToTimeString());

            historyAssets = _settings.HistoryHolder.Assets;

            // Load history forach asset pair
            foreach (var asset in historyAssets)
            {
                _holder.Add(asset, new LinkedList<Price>());

                // Get FromDate Ignoring weekends
                var historyStart = GetHistoryStartDate(DateTime.UtcNow);
                                
                StartLog += $"\n\r{DateTime.UtcNow.ToTimeString()} | GetHistory({asset}_{historyStart.ToDateTimeString()})";
                var tmp = await _assetDatabase.GetAssetHistory(historyStart, DateTime.UtcNow, asset);                
                StartLog += $"\n\r{DateTime.UtcNow.ToTimeString()} | GetHistory({asset}_{historyStart.ToDateTimeString()}) DONE";

                foreach (var historyItem in tmp)
                {
                    _holder[asset].AddLast( new Price()
                    {
                        Ask = historyItem.BestAsk.Value,
                        Bid = historyItem.BestBid.Value,
                        Date = historyItem.Timestamp
                    } );
                }
                StartLog += $"\n\r{DateTime.UtcNow.ToTimeString()} | Build Cache({asset}_{_holder[asset].Count} items) DONE";
            }
            Console.WriteLine(StartLog);
            await _log?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, StartLog, DateTime.UtcNow);

            // Start subscribing prices
            _subscriber.MessageReceived += Subscriber_MessageReceived;
            isStarting = false;
            InitializationFinished?.Invoke(this, new EventArgs());
        }

        private DateTime GetHistoryStartDate(DateTime historyEnd)
        {
            var currentDay = GetLastWeekDay(historyEnd);
            for (int i = 0; i < _settings.HistoryHolder.NumberOfDaysInCache-1; i++)
            {
                var nextDay = GetLastWeekDay(currentDay.AddDays(-1));
                currentDay = nextDay;
            }
            return currentDay.Date;
        }

        private DateTime GetLastWeekDay(DateTime date)
        {
            DateTime retval = new DateTime(date.Ticks);
            while ((int)retval.DayOfWeek < 1 || (int)retval.DayOfWeek > 5)
            {
                retval = retval.AddDays(-1);
            }
            return retval;
        }

        private void Subscriber_MessageReceived(object sender, IInstrumentPrice e)
        {
            if (!historyAssets.Contains(e.Instrument))
                return;
            lock (HistoryLock)
            {
                if (!_holder.ContainsKey(e.Instrument))
                    _holder.Add(e.Instrument, new LinkedList<Price>());

                _holder[e.Instrument].AddLast(new Price()
                {
                    Ask = e.Ask,
                    Bid = e.Bid,
                    Date = e.Date
                });
                DateTime HistoryStart = GetHistoryStartDate(DateTime.UtcNow);

                if (_holder[e.Instrument].First.Value.Date < HistoryStart)
                    _holder[e.Instrument].RemoveFirst();
            }
        }

        public Price[] GetHistory(string asset)
        {
            // Still build history
            if (isStarting)
                return new Price[0];

            // Asset not in history
            if (!_holder.ContainsKey(asset))
                return new Price[0];
            else
            {
                lock (HistoryLock)
                {
                    Price[] copy = new Price[_holder[asset].Count];
                    _holder[asset].CopyTo(copy, 0);
                    return copy;
                }
            }
        }

        public void Dispose()
        {
            _subscriber.MessageReceived -= Subscriber_MessageReceived;
            _holder.Clear();
        }

        

    }
}
