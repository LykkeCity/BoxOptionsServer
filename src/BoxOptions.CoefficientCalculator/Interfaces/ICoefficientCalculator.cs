using BoxOptions.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace BoxOptions.CoefficientCalculator
{
    /// <summary>
    /// Coefficient Calculator Interface
    /// </summary>
    public interface ICoefficientCalculator: IDisposable
    {

        void Init(IAssetQuoteSubscriber quoteSubscriber, IAssetDatabase historyRep);

        void StartSubscriber();
        void StopSubscriber();

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
        Task<string> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex, string userId);
        /// <summary>
        /// API Request method call
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        Task<string> RequestAsync(string pair, string userId);
    }
}
