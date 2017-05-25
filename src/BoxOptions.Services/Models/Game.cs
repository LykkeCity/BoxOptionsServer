using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Game
    {
        readonly string assetPair;

        int currentStatus;
        DateTime creationDate;
        

        GameParameters parameters;

        List<GameParametersHistory> parameterHistory;
        
        Dictionary<Box, decimal> betList;

        public Game(string assetPair)
        {
            this.assetPair = assetPair;
        }

        public string AssetPair { get => assetPair;  }
    }
    
}
