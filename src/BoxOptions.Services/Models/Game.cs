using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class Game
    {
        int currentStatus;
        DateTime creationDate;
        string assetPair;

        GameParameters parameters;

        List<GameParametersHistory> parameterHistory;
        List<int> statusHistory;
        Dictionary<Box, decimal> betList;



    }
}
