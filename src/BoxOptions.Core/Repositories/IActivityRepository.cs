using BoxOptions.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IActivityRepository
    {
        Task InsertActivityAsync(IEnumerable<IActivity> entities);
        Task<IActivity> GetActivity(string instrument, string name);
        Task<IEnumerable<IActivity>> GetActivities();
        Task<IEnumerable<IActivity>> GetActivityByInstrument();
    }
}
