using System.Collections.Generic;
using BoxOptions.Core;
using WampSharp.V2.Rpc;

namespace BoxOptions.Services
{
    public interface IRpcMethodsService
    {
        [WampProcedure("init.chartdata")]
        Dictionary<string, GraphBidAskPair[]> InitChardData();

        [WampProcedure("init.assets")]
        AssetPair[] InitAssets();
    }

    public class RpcMethodsService : IRpcMethodsService
    {
        private readonly IMicrographCacheService _micrographCacheService;

        public RpcMethodsService(IMicrographCacheService micrographCacheService)
        {
            _micrographCacheService = micrographCacheService;
        }

        public Dictionary<string, GraphBidAskPair[]> InitChardData()
        {
            return _micrographCacheService.GetGraphData();
        }

        public AssetPair[] InitAssets()
        {
            return new[]
            {
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "BTC",
                    Id = "BTCCHF",
                    Name = "BTC/CHF",
                    QuoteAssetId = "CHF"

                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "BTC",
                    Id = "BTCEUR",
                    Name = "BTC/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "BTC",
                    Id = "BTCGBP",
                    Name = "BTC/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 2,
                    BaseAssetId = "BTC",
                    Id = "BTCJPY",
                    Name = "BTC/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "BTC",
                    Id = "BTCLKK",
                    Name = "BTC/LYKKE",
                    QuoteAssetId = "LKK"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "BTC",
                    Id = "BTCUSD",
                    Name = "BTC/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "CHF",
                    Id = "CHFJPY",
                    Name = "CHF/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "ETH",
                    Id = "ETHBTC",
                    Name = "ETH/BTC",
                    QuoteAssetId = "BTC"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "ETH",
                    Id = "ETHLKK",
                    Name = "ETH/LYKKE",
                    QuoteAssetId = "LKK"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "ETH",
                    Id = "ETHUSD",
                    Name = "ETH/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "EUR",
                    Id = "EURCHF",
                    Name = "EUR/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "EUR",
                    Id = "EURGBP",
                    Name = "EUR/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "EUR",
                    Id = "EURJPY",
                    Name = "EUR/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "EUR",
                    Id = "EURUSD",
                    Name = "EUR/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "FCHI",
                    Id = "FCHIEUR",
                    Name = "FCHI/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "GBP",
                    Id = "GBPCHF",
                    Name = "GBP/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "GBP",
                    Id = "GBPJPY",
                    Name = "GBP/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "GBP",
                    Id = "GBPUSD",
                    Name = "GBP/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "GDAXI",
                    Id = "GDAXIEUR",
                    Name = "GDAXI/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "HSI",
                    Id = "HSIHKD",
                    Name = "HSI/HKD",
                    QuoteAssetId = "HKD"
                },
                new AssetPair
                {
                    Accuracy = 2,
                    BaseAssetId = "ICO",
                    Id = "ICOCHF",
                    Name = "ICO/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "ICO",
                    Id = "ICOEUR",
                    Name = "ICO/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "ICO",
                    Id = "ICOGBP",
                    Name = "ICO/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "ICO",
                    Id = "ICOUSD",
                    Name = "ICO/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "LKK",
                    Id = "LKKCHF",
                    Name = "LYKKE/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "LKK",
                    Id = "LKKEUR",
                    Name = "LYKKE/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "LKK",
                    Id = "LKKGBP",
                    Name = "LYKKE/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "LKK",
                    Id = "LKKJPY",
                    Name = "LYKKE/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "LKK",
                    Id = "LKKUSD",
                    Name = "LYKKE/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLRBTC",
                    Name = "SLR/BTC",
                    QuoteAssetId = "BTC"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLRCHF",
                    Name = "SLR/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLREUR",
                    Name = "SLR/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLRGBP",
                    Name = "SLR/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLRJPY",
                    Name = "SLR/JPY",
                    QuoteAssetId = "JPY"
                },
                new AssetPair
                {
                    Accuracy = 8,
                    BaseAssetId = "SLR",
                    Id = "SLRUSD",
                    Name = "SLR/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "STOXX50E",
                    Id = "STOXX50EEUR",
                    Name = "STOXX50E/EUR",
                    QuoteAssetId = "EUR"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "STOXX50E",
                    Id = "STOXX50EUSD",
                    Name = "STOXX50E/USD",
                    QuoteAssetId = "USD"
                },
                new AssetPair
                {
                    Accuracy = 6,
                    BaseAssetId = "UK100",
                    Id = "UK100GBP",
                    Name = "UK100/GBP",
                    QuoteAssetId = "GBP"
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "USD",
                    Id = "USDCHF",
                    Name = "USD/CHF",
                    QuoteAssetId = "CHF"
                },
                new AssetPair
                {
                    Accuracy = 3,
                    BaseAssetId = "USD",
                    Id = "USDJPY",
                    Name = "USD/JPY",
                    QuoteAssetId = "JPY"
                }
            };

        }
    }
}
