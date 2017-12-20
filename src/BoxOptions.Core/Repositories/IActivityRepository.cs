using BoxOptions.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core.Repositories
{
    public interface IActivityRepository
    {
        Task InsertOrReplaceActivities(IEnumerable<IActivity> entities);
        Task<IActivity> GetActivityById(string id);        
        Task<IEnumerable<IActivity>> GetActivitiesByInstrument(string instrument);
        Task<IActivity> GetActivitiesByInstrumentAndName(string instrument, string name);
    }
}
