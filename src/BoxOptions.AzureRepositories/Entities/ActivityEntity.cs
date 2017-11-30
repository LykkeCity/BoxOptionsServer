using BoxOptions.Core.Interfaces;
using System.Linq;

namespace BoxOptions.AzureRepositories.Entities
{
    public class ActivityEntity : IActivity
    {
        public string ActivityArray { get; set; }
        public string Name { get; set; }
        public string Instrument { get; set; }

        decimal[] IActivity.ActivityArray { get =>GetActivityArray(); }

        private decimal[] GetActivityArray()
        {
            var res = ActivityArray.Split("\n\r")
                .Select(x => decimal.Parse(x, System.Globalization.CultureInfo.InvariantCulture));
            return res.ToArray();
        }

        public static string GetPartitionKey(IActivity src)
        {
            return src.Instrument;
        }
        public static string GetRowKey(IActivity src)
        {
            return src.Name;
        }
    }
}
