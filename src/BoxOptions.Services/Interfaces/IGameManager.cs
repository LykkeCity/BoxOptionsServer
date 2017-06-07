using BoxOptions.Services.Models;
using System;

namespace BoxOptions.Services.Interfaces
{
    public interface IGameManager
    {           
        void SetUserParameters(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        string RequestUserCoeff(string userId, string pair);        
        
    }
}
