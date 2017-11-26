using Autofac;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Core;
using BoxOptions.Core.Models;
using BoxOptions.Services.Interfaces;
using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxOptions.Services
{
    public class HistoryHolder : IHistoryHolder,IStartable, IDisposable
    {
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

        public async void Start()
        {
            Console.WriteLine("{0} > History Holder Starting", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            await _log?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, "History Holder Started", DateTime.UtcNow);

            string StartLog = string.Format("{0} > Getting History", DateTime.UtcNow.ToString("HH:mm:ss.fff"));

            historyAssets = _settings.HistoryHolder.Assets;

            // Load 2 days history forach asset pair
            foreach (var asset in historyAssets)
            {
                _holder.Add(asset, new LinkedList<Price>());

                // Get FromDate Ignoring weekends
                var historyStart = GetHistoryStartDate(DateTime.UtcNow);
                                
                StartLog += $"\n\r{DateTime.UtcNow.ToString("HH:mm:ss.fff")} > GetHistory({asset}>{historyStart.ToString("yyyy-MM-dd")})";
                var tmp = await _assetDatabase.GetAssetHistory(historyStart, DateTime.UtcNow, asset);                
                StartLog += $"\n\r{DateTime.UtcNow.ToString("HH:mm:ss.fff")} > GetHistory({asset}>{historyStart.ToString("yyyy-MM-dd")}) DONE";

                foreach (var historyItem in tmp)
                {
                    _holder[asset].AddLast( new Price()
                    {
                        Ask = historyItem.BestAsk.Value,
                        Bid = historyItem.BestBid.Value,
                        Date = historyItem.Timestamp
                    } );
                }
                StartLog += $"\n\r{DateTime.UtcNow.ToString("HH:mm:ss.fff")} > Build Cache({asset}>{_holder[asset].Count} items) DONE";
            }
            Console.WriteLine(StartLog);
            await _log?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, StartLog, DateTime.UtcNow);

            // Start subscribing prices
            _subscriber.MessageReceived += Subscriber_MessageReceived;
            isStarting = false;
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

        private void Subscriber_MessageReceived(object sender, InstrumentPrice e)
        {
            if (!historyAssets.Contains(e.Instrument))
                return;

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

        public LinkedList<Price> GetHistory(string asset)
        {
            // Still build history
            if (isStarting)
                return new LinkedList<Price>();

            // Asset not in history
            if (!_holder.ContainsKey(asset))
                return new LinkedList<Price>();
            else
                return _holder[asset];

        }

        public void Dispose()
        {
            _subscriber.MessageReceived -= Subscriber_MessageReceived;
            _holder.Clear();
        }

        

    }
}
