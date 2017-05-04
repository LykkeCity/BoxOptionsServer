﻿using BoxOptions.Common;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;


namespace BoxOptions.Public.Processors
{
    /// <summary>
    /// Coefficient calculator Using proxy to Coefficient API
    /// </summary>
    public class ProxyCoefficientCalculator : ICoefficientCalculator
    {
        /// <summary>
        /// Settings Object
        /// </summary>
        private readonly BoxOptionsSettings settings;

        /// <summary>
        /// Default constructor with ref to <see cref="BoxOptionsSettings"/>
        /// </summary>
        /// <param name="settings"></param>
        public ProxyCoefficientCalculator(BoxOptionsSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Change method forwarded to Coefficient API and returns result.
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="timeToFirstOption"></param>
        /// <param name="optionLen"></param>
        /// <param name="priceSize"></param>
        /// <param name="nPriceIndex"></param>
        /// <param name="nTimeIndex"></param>
        /// <returns></returns>
        public async Task<string> ChangeAsync(string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            string result = await $"{settings.BoxOptionsApi.CoefApiUrl}/change"
                .SetQueryParams(new
                {
                    pair,
                    timeToFirstOption,
                    optionLen,
                    priceSize,
                    nPriceIndex,
                    nTimeIndex
                })
                .GetStringAsync();
            return result;
        }
        /// <summary>
        /// Request method forwarded to Coefficient API and returns result.
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public async Task<string> RequestAsync(string pair)
        {
            //TODO: UserId??
            string result = await $"{settings.BoxOptionsApi.CoefApiUrl}/request"
                .SetQueryParams(new { pair })
                .GetStringAsync();
            return result;
        }
    }
}