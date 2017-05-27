using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Box
    {
        readonly string id;
        decimal minPrice;
        decimal maxPrice;
        int timeToGraph; // (in seconds), 
        int timeLength;//(in seconds), 
        decimal coefficient;
        //decimal betAmount;
        
        public Box(string id)
        {
            this.id = id;
        }
        public string Id => id;

        public decimal MinPrice { get => minPrice; set => minPrice = value; }
        public decimal MaxPrice { get => maxPrice; set => maxPrice = value; }
        public int TimeToGraph { get => timeToGraph; set => timeToGraph = value; }
        public int TimeLength { get => timeLength; set => timeLength = value; }
        public decimal Coefficient { get => coefficient; set => coefficient = value; }
        //public decimal BetAmount { get => betAmount; set => betAmount = value; }
    }
}
