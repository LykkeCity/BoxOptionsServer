using BoxOptions.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class TestCoeffCalc : ICoefficientCalculator
    {
        private readonly IEnumerable<ICoefficientCalculator> _calcs;

        public TestCoeffCalc(IEnumerable<ICoefficientCalculator> calcs)
        {
            _calcs = calcs;
        }

        public Task<string> ChangeAsync(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RequestAsync(string userId, string pair)
        {
            List<string> res = new List<string>();
            foreach (var calc in _calcs)
            {
                res.Add(await calc.RequestAsync(userId, pair));
            }
            return res[0];
        }

        public bool ValidateChange(string userId, string pair, int timeToFirstOption, int optionLen, double priceSize, int nPriceIndex, int nTimeIndex)
        {
            throw new NotImplementedException();
        }

        public bool ValidateChangeResult(string result, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool ValidateRequest(string userId, string pair)
        {
            throw new NotImplementedException();
        }

        public bool ValidateRequestResult(string result, out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
