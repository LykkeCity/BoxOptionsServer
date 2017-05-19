using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.Processors
{
    /// <summary>
    /// Coefficient Calculator Interface
    /// </summary>
    public interface ICoefficientCalculator
    {
        /// <summary>
        /// API Change method call
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="timeToFirstOption"></param>
        /// <param name="optionLen"></param>
        /// <param name="priceSize"></param>
        /// <param name="nPriceIndex"></param>
        /// <param name="nTimeIndex"></param>
        /// <returns></returns>
        Task<string> ChangeAsync(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        /// <summary>
        /// API Request method call
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        Task<string> RequestAsync(string userId, string pair);

        /// <summary>
        /// Validate Request 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool ValidateRequest(string userId, string pair);
        bool ValidateRequestResult(string result);

        bool ValidateChange(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex);
        bool ValidateChangeResult(string result);
    }
}
