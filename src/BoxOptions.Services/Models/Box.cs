using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Box
    {
        readonly string id;

        // TODO: box props
        decimal coeff;

        public Box()
        {
            id = Guid.NewGuid().ToString();
        }
        public string Id => id;
        public decimal Coeff { get => coeff; set => coeff = value; }

        
    }
}
