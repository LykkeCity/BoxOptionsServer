using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace BoxOptions.AzureRepositories.Entities
{
    public class ActivityEntity : TableEntity, IActivity
    {
        public string Activity { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Instrument { get; set; }
        public bool IsDefault { get; set; }

        public double[] ActivityArray { get =>GetActivityArray(); set => SetActivityArray(value); }

        private void SetActivityArray(double[] value)
        {
            Activity = string.Join(";", value.Select(x=>x.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }
        private double[] GetActivityArray()
        {
            var res = Activity.Split(";")
                .Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture));
            return res.ToArray();
        }

        public static string GetPartitionKey(IActivity src)
        {
            return src.Instrument;
        }
        public static string GetRowKey(IActivity src)
        {
            return src.Id;
        }

        public static ActivityEntity CreateEntity(IActivity src)
        {
            return new ActivityEntity
            {
                Id = src.Id,
                PartitionKey = GetPartitionKey(src),
                RowKey = GetRowKey(src),
                ActivityArray = src.ActivityArray,
                Instrument = src.Instrument,
                IsDefault = src.IsDefault,
                Name = src.Name
            };
        }
    }
}
