using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IActivityManager
    {        
        Task<IEnumerable<IActivity>> GetActivities(string instrument);
        Task<IActivity> GetActivityByName(string instrument, string name);
        Task SetActivity(IActivity activity);

        Task<IActivity> GetDefaultActivity(string instrument);
        Task SetDefaultActivity(IActivity activity);
    }
}
