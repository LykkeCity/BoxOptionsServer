using Autofac;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class LocalFSHistory : IBoxOptionsHistory
    {                           
        static System.Globalization.CultureInfo Ci = new System.Globalization.CultureInfo("en-us");
        static object AssetFileAccessLock = new object();
        static object UserFileAccessLock = new object();
                
        private Task<LinkedList<AssetQuote>> LoadAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            LinkedList<AssetQuote> retval = new LinkedList<AssetQuote>();            
            try
            {
                lock (AssetFileAccessLock)
                {
                    DateTime currentDay = dateFrom;
                    do
                    {
                        string currentFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"boxoptions.assets{currentDay.ToString("yyyyMMdd")}.hist");
                        if (System.IO.File.Exists(currentFile))
                        {
                            using (var filestream = System.IO.File.OpenRead(currentFile))
                            using (var textstream = new System.IO.StreamReader(filestream))
                            {
                                while (!textstream.EndOfStream)
                                {

                                    string assetEntry = textstream.ReadLine();
                                    string[] values = assetEntry.Split('|');
                                    // Filter Asset Pair 
                                    if (values[1] == assetPair)
                                    {
                                        // Filter Date
                                        DateTime dt = DateTime.ParseExact(values[0], "yyyyMMdd_HHmmssff", Ci);
                                        if (dt >= dateFrom && dt <= dateTo)
                                        {
                                            retval.AddLast(new AssetQuote()
                                            {
                                                Timestamp = dt,
                                                AssetPair = values[1],
                                                IsBuy = values[2] == "1" ? true : false,
                                                Price = double.Parse(values[3], Ci)
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        currentDay = currentDay.AddDays(1);
                    } while (currentDay <= dateTo);
                }
            }
            catch
            {
                throw;
            }
            return Task.FromResult(retval);
        }        
       
        private Task AddToAssetFile(string line)
        {
            lock (AssetFileAccessLock)
            {
                string assetHistoryFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"boxoptions.assets{DateTime.UtcNow.ToString("yyyyMMdd")}.hist");
                if (!System.IO.File.Exists(assetHistoryFile))
                {
                    var historystream = System.IO.File.Create(assetHistoryFile);
                    historystream.Dispose();
                }

                using (var filestream = new System.IO.FileStream(assetHistoryFile, System.IO.FileMode.Append,System.IO.FileAccess.Write))
                using (var textstream = new System.IO.StreamWriter(filestream))
                {                    
                    textstream.WriteLine(line);
                }
            }
            return Task.FromResult(0);
        }

        public Task<LinkedList<AssetQuote>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            return LoadAssetHistory(dateFrom, dateTo, assetPair);
        }

        Task IBoxOptionsHistory.AddToAssetHistory(AssetQuote quote)
        {
            string line = string.Format("{0}|{1}|{2}|{3}", quote.Timestamp.ToString("yyyyMMdd_HHmmssff", Ci), quote.AssetPair, quote.IsBuy ? "1" : "0", quote.Price.ToString(Ci));
            try
            {
                return AddToAssetFile(line);
            }
            catch
            {
                throw;
            }
        }
    }
}
