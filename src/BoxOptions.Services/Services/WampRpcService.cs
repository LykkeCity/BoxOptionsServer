using BoxOptions.Core.Models;
using BoxOptions.Services.Interfaces;
using System.Collections.Generic;
using System;
using BoxOptions.Services.Models;
using Common.Log;
using BoxOptions.Core;
using System.Threading.Tasks;
using System.Globalization;
using Common;

namespace BoxOptions.Services
{
    /// <summary>
    /// Wamp RPC Host
    /// </summary>
    public class WampRpcService : IRpcMethods
    {
        private readonly IMicrographCache _micrographCacheService;
        private readonly IGameManager _gameManager;
        private readonly ILog _log;
        private readonly ILogRepository _logRepository;

        public WampRpcService(IMicrographCache micrographCacheService, IGameManager gameManager, ILog log, ILogRepository logRepository)
        {
            _micrographCacheService = micrographCacheService;
            _gameManager = gameManager;
            _log = log;
            _logRepository = logRepository;

            LogInfo("Wamp Rpc Service Started");
        }

        public Dictionary<string, Price[]> InitChartData()
        {
            // Request data from RabbitMq and forward it.

            try
            {
                return _micrographCacheService.GetGraphData();
            }
            catch (Exception ex)
            {
                LogError(ex, "InitChartData");
                return null;
            }
        }

        /// <summary>
        /// Initial Data for charts
        /// </summary>
        /// <returns></returns>
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
                },
                new AssetPair
                {
                    Accuracy = 5,
                    BaseAssetId = "EUR",
                    Id = "EURAUD",
                    Name = "EUR/AUD",
                    QuoteAssetId = "AUD"
                },
            };

        }

        public string InitUser(string userId)
        {
            try
            {
                var res = _gameManager.InitUser(userId);
                string retval = res.ToJson();
                return retval;
            }
            catch (Exception ex)
            {
                LogError(ex, "InitUser");
                return ex.Message;
            }            
        }

        public PlaceBetResult PlaceBet(string userId, string assetPair, string box, decimal betValue)
        {
            try
            {

                DateTime betdate = _gameManager.PlaceBet(userId, assetPair, box, betValue);
                return new PlaceBetResult()
                {
                    BetTimeStamp = betdate,
                    Status = "OK"
                };
            }
            catch (Exception ex)
            {
                LogError(ex, "PlaceBet");
                return new PlaceBetResult()
                {
                    BetTimeStamp = DateTime.MinValue,
                    Status = ex.Message
                };
            }
        }

        public decimal GetBalance(string userId)
        {
            try
            {
                return _gameManager.GetUserBalance(userId);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetBalance");
                return -1;
            }
        }

        public string SetBalance(string userId, decimal balance)
        {
            try
            {
                _gameManager.SetUserBalance(userId, balance);
                return "OK";
            }
            catch (Exception ex)
            {
                LogError(ex, "SetBalance");
                return ex.Message;
            }
        }

        public string ChangeParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            try
            {
                _gameManager.SetUserParameters(userId, pair, timeToFirstOption, optionLen, priceSize, nPriceIndex, nTimeIndex);
                return "OK";
            }
            catch (Exception ex)
            {
                LogError(ex, "ChangeParameters");
                return ex.Message;
            }
        }
        public CoeffParameters GetParameters(string userId, string pair)
        {
            try
            {
                return _gameManager.GetUserParameters(userId, pair);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetParameters");
                return null;
            }
        }

        public string RequestCoeff(string userId, string pair)
        {
            try
            {
                return _gameManager.RequestUserCoeff(userId, pair);
            }
            catch (Exception ex)
            {
                LogError(ex, "RequestCoeff");
                return ex.Message;
            }
        }

        public string SaveLog(string userId, string eventCode, string message)
        {
            try
            {
                Task t = _logRepository?.InsertAsync(new LogItem()
                {
                    ClientId = userId,
                    EventCode = eventCode,
                    Message = message
                });
                t.Wait();

                _gameManager.AddUserLog(userId, eventCode, message);

                return "OK";
            }
            catch (Exception ex)
            {
                LogError(ex, "RequestCoeff");
                return ex.Message;
            }
        }

        private void LogInfo(string message, string sender = "this")
        {
            _log?.WriteInfoAsync("WampRpcService", sender, "", message);
        }
        private void LogError(Exception ex, string sender = "this")
        {
            _log?.WriteErrorAsync("WampRpcService", sender, "", ex);
        }
                



        //public string GameStart(string userId, string assetPair)
        //{
        //    try
        //    {
        //        return _gameManager.GameStart(userId, assetPair);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex, "GameStart");
        //        return ex.Message;
        //    }
        //}

        //public string GameClose(string userId)
        //{
        //    try
        //    {
        //        _gameManager.GameClose(userId);
        //        return "OK";
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex, "GameClose");
        //        return ex.Message;
        //    }
        //}
    }
}
