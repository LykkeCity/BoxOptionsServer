using Autofac;
using BoxOptions.Common;
using BoxOptions.Core;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class MicrographCacheService: IMicrographCache, IStartable, IDisposable
    {
        /// <summary>
        /// Settings Object        
        /// </summary>
        private readonly BoxOptionsSettings settings;
        /// <summary>
        /// Asset Quote Subscriber
        /// </summary>
        private readonly IAssetQuoteSubscriber subscriber;
        /// <summary>
        /// Asset Graphic data
        /// </summary>
        private readonly Dictionary<string, List<GraphBidAskPair>> graphData;
       
        /// <summary>
        /// Static thread lock mutex for Graph Operations
        /// </summary>
        private static readonly object GraphQueueLock = new object();

        public MicrographCacheService(BoxOptionsSettings settings, IAssetQuoteSubscriber subscriber)
        {
            this.settings = settings;
            
            this.subscriber = subscriber;
            graphData = new Dictionary<string, List<GraphBidAskPair>>();
        }
        
        /// <summary>
        /// Start receiving events from IAssetQuoteSubscriber
        /// </summary>
        public void Start()
        {
            // Register event catching from subscriber
            subscriber.MessageReceived += Subscriber_MessageReceived;
        }
        /// <summary>
        /// Dispose MicrographCacheService Instance
        /// </summary>
        public void Dispose()
        {
            // Un-register event catching from subscriber
            subscriber.MessageReceived -= Subscriber_MessageReceived;
        }

        /// <summary>
        /// Event Handler for messages received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Subscriber_MessageReceived(object sender, AssetPairBid e)
        {
            try { await ProcessPrice(e); }
            catch
            {
                //TODO: Log Error
                throw;
            }
        }
        
        /// <summary>
        /// Process AssetQuote received from IAssetQuoteSubscriber
        /// </summary>
        /// <param name="assetQuote"></param>
        /// <returns></returns>
        private Task ProcessPrice(AssetPairBid assetBid)
        {
            // Parameter validation
            if (assetBid == null ||
                string.IsNullOrEmpty(assetBid.Id) ||
                assetBid.Ask <= 0 ||
                assetBid.Bid <= 0)
            {
                return Task.FromResult(0);
            }

            // Add Received Asset Quote to Graph Data
            lock (GraphQueueLock)
            {
                // If assetbid has Ask and Bid prices, add it to Graphdata
                if (assetBid.Ask > 0 && assetBid.Bid > 0)
                {
                    // Add AssetPair to graph;
                    if (!graphData.ContainsKey(assetBid.Id))
                    {
                        graphData.Add(assetBid.Id, new List<GraphBidAskPair>());
                    }

                    // Add Quote Data
                    graphData[assetBid.Id].Add(new GraphBidAskPair
                    {
                        Bid = assetBid.Bid,
                        Ask = assetBid.Ask,
                        Date = assetBid.Date
                    });

                    // If quote data array is to big, resize it.
                    if (graphData[assetBid.Id].Count > settings.BoxOptionsApi.PricesSettingsBoxOptions.GraphPointsCount)
                    {
                        graphData[assetBid.Id] = graphData[assetBid.Id]
                            .GetRange(1, graphData[assetBid.Id].Count - 1);
                    }
                }
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Return clone of graph data dictionary
        /// <returns></returns>
        public Dictionary<string, GraphBidAskPair[]> GetGraphData()
        {
            // Mutex lock Graph Data manipulation
            lock (GraphQueueLock)
            {
                // Create clone array
                var copy = new Dictionary<string, GraphBidAskPair[]>();

                // Copy existing data to clone array
                foreach (var pair in graphData)
                {
                    var pairs = new List<GraphBidAskPair>();

                    foreach (var bidAsk in pair.Value)
                    {
                        pairs.Add(new GraphBidAskPair
                        {
                            Ask = bidAsk.Ask,
                            Bid = bidAsk.Bid,
                            Date = bidAsk.Date
                        });
                    }

                    copy.Add(pair.Key, pairs.ToArray());
                }
                // Return clone
                return copy;
            }
        }
    }
}
