using Autofac;
using BoxOptions.Common.Interfaces;
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
        private readonly Dictionary<string, LinkedList<Price>> holder;
        private readonly IAssetQuoteSubscriber subscriber;
        private readonly IAssetRepository assetRepo;
        private readonly IBoxConfigRepository boxRepo;
        private readonly ILog appLog;

        private string[] historyAssets;



        bool isStarting;

        public HistoryHolder(IAssetQuoteSubscriber subscriber, IAssetRepository assetRepo,IBoxConfigRepository boxRepo, ILog appLog)
        {
            this.assetRepo = assetRepo;
            this.appLog = appLog;
            this.subscriber = subscriber;
            this.boxRepo = boxRepo;
            holder = new Dictionary<string, LinkedList<Price>>();
            isStarting = true;

        }

        public async void Start()
        {
            await appLog?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, "History Holder Started", DateTime.UtcNow);

            string StartLog = string.Format("{0} > Getting History", DateTime.UtcNow.ToString("HH:mm:ss.fff")); 
            var boxCfg = await boxRepo.GetAll();

            string logLine = string.Format("{0} > GetAssets Done", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            StartLog += "\n\r" + logLine;
            Console.WriteLine(logLine);

            historyAssets = boxCfg.Where(a => a.SaveHistory).Select(m => m.AssetPair).ToArray();
            
            // Load 2 days history forach asset pair
            foreach (var asset in historyAssets)
            {
                holder.Add(asset, new LinkedList<Price>());

                // Ignore weekends
                DateTime FirstDay = GetLastWeekDay(DateTime.UtcNow.Date);
                DateTime SecondDay = GetLastWeekDay(FirstDay.AddDays(-1));

                logLine = string.Format("{0} > GetHistory({1}>{2}|{3})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), asset, SecondDay.ToString("yyyy-MM-dd"), FirstDay.ToString("yyyy-MM-dd"));
                StartLog += "\n\r" + logLine;
                //Console.WriteLine(logLine);

                var tmp = await assetRepo.GetRange(SecondDay, FirstDay, asset);

                logLine = string.Format("{0} > GetHistory({1}>{2}|{3}) DONE", DateTime.UtcNow.ToString("HH:mm:ss.fff"), asset, SecondDay.ToString("yyyy-MM-dd"), FirstDay.ToString("yyyy-MM-dd"));
                //Console.WriteLine(logLine);
                StartLog += "\n\r" + logLine;
                
                foreach (var historyItem in tmp)
                {
                    holder[asset].AddLast( new Price()
                    {
                        Ask = historyItem.BestAsk.Value,
                        Bid = historyItem.BestBid.Value,
                        Date = historyItem.Timestamp
                    } );
                }

                logLine = string.Format("{0} > Build Cache({1}>{2} items) DONE", DateTime.UtcNow.ToString("HH:mm:ss.fff"), asset, holder[asset].Count);
                //Console.WriteLine(logLine);
                StartLog += "\n\r" + logLine;
            }            
            await appLog?.WriteInfoAsync("BoxOptions.Services.HistoryHolder", "Start", null, StartLog, DateTime.UtcNow);

            // Start subscribing prices
            subscriber.MessageReceived += Subscriber_MessageReceived;
            isStarting = false;
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

            if (!holder.ContainsKey(e.Instrument))
                holder.Add(e.Instrument, new LinkedList<Price>());

            holder[e.Instrument].AddLast(new Price()
            {
                Ask = e.Ask,
                Bid = e.Bid,
                Date = e.Date
            });

            DateTime FirstDay = GetLastWeekDay(DateTime.UtcNow.Date);
            DateTime SecondDay = GetLastWeekDay(FirstDay.AddDays(-1));


            if (holder[e.Instrument].First.Value.Date < SecondDay)
                holder[e.Instrument].RemoveFirst();


        }

        public LinkedList<Price> GetHistory(string asset)
        {
            // Still build history
            if (isStarting)
                return new LinkedList<Price>();

            // Asset not in history
            if (!holder.ContainsKey(asset))
                return new LinkedList<Price>();
            else
                return holder[asset];

        }

        public void Dispose()
        {
            subscriber.MessageReceived -= Subscriber_MessageReceived;
            holder.Clear();
        }

        

    }
}
