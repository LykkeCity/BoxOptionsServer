using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Services.Models
{
    public class PlaceBetResult : IPlaceBetResult
    {        
        public DateTime BetTimeStamp { get; set; }
        public string Status { get; set; }
    }
}
