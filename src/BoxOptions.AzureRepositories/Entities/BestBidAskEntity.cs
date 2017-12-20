using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BoxOptions.AzureRepositories.Entities
{
    public class BestBidAskEntity : TableEntity, IBestBidAsk
    {
        public string Asset { get; set; }
        public double? BestAsk { get; set; }
        public double? BestBid { get; set; }
        public string Source { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime BidDate { get; set; }

        DateTime IBestBidAsk.Timestamp => BidDate;

        public static string GetPartitionKey(IBestBidAsk src)
        {
            string key = string.Format("{0}_{1}", src.Asset, src.ReceiveDate.ToString("yyyyMMdd_HH"));
            return key;
        }
        public static string GetRowKey(IBestBidAsk src)
        {
            string key = src.ReceiveDate.Ticks.ToString();
            return key;
        }

        public static BestBidAskEntity CreateEntity(IBestBidAsk src)
        {
            return new BestBidAskEntity
            {
                PartitionKey = GetPartitionKey(src),
                RowKey = GetRowKey(src),
                Asset = src.Asset,
                BestAsk = src.BestAsk,
                BestBid = src.BestBid,
                BidDate = src.Timestamp,
                Source = src.Source,
                ReceiveDate = src.ReceiveDate
            };
        }

        public static IBestBidAsk CreateDto(BestBidAskEntity src)
        {
            long ticks = long.Parse(src.RowKey);
            DateTime rdate = new DateTime(ticks, DateTimeKind.Utc);
            return new BestBidAsk
            {
                Asset = src.Asset,
                BestAsk = src.BestAsk,
                BestBid = src.BestBid,
                Timestamp = src.BidDate != DateTime.MinValue ? src.BidDate : rdate,
                ReceiveDate = rdate,
                Source = src.Source
            };
        }
    }
}
