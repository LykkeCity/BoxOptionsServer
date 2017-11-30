using BoxOptions.Core.Interfaces;

namespace BoxOptions.Common.Models
{
    public class Activity : IActivity
    {
        public string Name { get; set; }
        public string Instrument { get; set; }
        public decimal[] ActivityArray { get; set; }
    }
}
