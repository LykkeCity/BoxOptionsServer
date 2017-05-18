using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Core
{    
    public interface IAssetItem
    {
        string AssetPair { get;  }
        bool IsBuy { get; }
        double Price { get; }
        DateTime Date { get; }
    }

    public class AssetItem : IAssetItem
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string ServerTimestamp { get; set; }
    }

    public interface IAssetRepository
    {
        Task<IAssetItem> InsertAsync(IAssetItem olapEntity);
        Task InsertManyAsync(IEnumerable<IAssetItem> olapEntities);
        Task<IEnumerable<AssetItem>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair);

    }
}
