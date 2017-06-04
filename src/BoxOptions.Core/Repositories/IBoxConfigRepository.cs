using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IBoxConfigRepository
    {
        Task InsertAsync(Interfaces.IBoxSize olapEntity);
        Task InsertManyAsync(IEnumerable<Interfaces.IBoxSize> olapEntities);
        Task<BoxSize> GetAsset(string assetPair);
        Task<IEnumerable<BoxSize>> GetAll();
    }
}
