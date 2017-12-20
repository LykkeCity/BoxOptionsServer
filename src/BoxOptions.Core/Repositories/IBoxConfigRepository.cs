using BoxOptions.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core.Repositories
{
    public interface IBoxConfigRepository
    {
        Task InsertAsync(IBoxSize olapEntity);
        Task InsertManyAsync(IEnumerable<IBoxSize> olapEntities);
        Task<IBoxSize> GetAsset(string assetPair);
        Task<IEnumerable<IBoxSize>> GetAll();
    }
}
