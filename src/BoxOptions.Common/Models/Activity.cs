using BoxOptions.Core.Interfaces;

namespace BoxOptions.Common.Models
{
    public class Activity : IActivity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Instrument { get; set; }
        public double [] ActivityArray { get; set; }
        public bool IsDefault { get; set; }

        
    }
}
