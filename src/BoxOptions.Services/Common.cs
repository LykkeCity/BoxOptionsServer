using BoxOptions.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services
{
    internal class Common
    {
        internal const bool ASK = false; // Bid => IsBuy == true

        /// <summary>
        /// Assets Allowed
        /// </summary>
        /// <returns></returns>
        public static string[] AllowedAssets { get { return new[] { "EURUSD", "EURAUD", "EURCHF", "EURGBP", "EURJPY", "USDCHF" }; } }
    }
}
