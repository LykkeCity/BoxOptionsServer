using AzureStorage;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class ActivityRepository: IActivityRepository
    {
        private readonly INoSQLTableStorage<ActivityEntity> _storage;

        public ActivityRepository(INoSQLTableStorage<ActivityEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<IActivity>> GetActivitiesByInstrument(string instrument)
        {
            var res = await _storage.GetDataAsync(instrument);
            return res;
        }

        public async Task<IActivity> GetActivitiesByInstrumentAndName(string instrument, string name)
        {
            var res = await _storage.GetDataAsync(instrument, e => e.Name == name);
            return res.FirstOrDefault();
        }

        public async Task<IActivity> GetActivityById(string id)
        {
            var res = await _storage.GetDataRowKeyOnlyAsync(id);
            return res.FirstOrDefault();
        }
                
        public async Task InsertOrReplaceActivities(IEnumerable<IActivity> entities)
        {
            await _storage.InsertOrReplaceBatchAsync(entities.Select(x => ActivityEntity.CreateEntity(x)));
        }
    }
}
