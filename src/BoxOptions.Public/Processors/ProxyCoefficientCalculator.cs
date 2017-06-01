using BoxOptions.Common;
using BoxOptions.Common.Interfaces;
using Flurl;
using Flurl.Http;
using System;
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
        public async Task<string> ChangeAsync(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            //TODO: UserId??
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
        public async Task<string> RequestAsync(string userId, string pair)
        {
            //TODO: UserId??            
            string result = await $"{settings.BoxOptionsApi.CoefApiUrl}/request"
                .SetQueryParams(new { pair, userId })
                .GetStringAsync();
            return result;
        }

        public bool ValidateChange(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            // Parameter validation


            return true;
        }

        public bool ValidateChangeResult(string result, out string errorMessage)
        {
            // TODO: Result validation
            errorMessage = "OK";
            return true;
        }

        public bool ValidateRequest(string userId, string pair)
        {
            return true;
        }

        public bool ValidateRequestResult(string result, out string errorMessage)
        {
            errorMessage = "OK";
            // Parse result from coefficient API into objects
            Models.CoefModels.CoefRequestResult res = Models.CoefModels.CoefRequestResult.Parse(result);

            ////Test: Force all to 1:
            //foreach (var block in res.CoefBlocks)
            //{
            //    foreach (var coef in block.Coeffs)
            //    {
            //        coef.HitCoeff = 1.0m;
            //        coef.MissCoeff = 1.0m;
            //    }
            //}

            ////Test: Force negative
            //foreach (var block in res.CoefBlocks)
            //{
            //    foreach (var coef in block.Coeffs)
            //    {
            //        coef.HitCoeff = -1.0m;
            //        break;
            //    }
            //    break;
            //}


            // Teste if all values are 1.0
            bool AllEqualOne = true;
            
            // Test if there are negative 
            bool NegativeCoef = false;
            foreach (var block in res.CoefBlocks)
            {
                foreach (var coef in block.Coeffs)
                {
                    if (coef.HitCoeff != 1.0m || coef.MissCoeff != 1.0m)
                    {
                        AllEqualOne = false;                        
                    }

                    if (coef.HitCoeff < 0 || coef.MissCoeff < 0)
                    {
                        NegativeCoef = true;
                    }
                }                
            }
            if (AllEqualOne == true) // All coefficients are 1.0 thow exception with information.
            {
                errorMessage = "All coefficients are equal to 1.0";
                return false;
            }

            if (NegativeCoef == true) // Negative coefficiente
            {
                errorMessage = "Negative coefficients";
                return false;
            }

            return true;
        }
    }
}
