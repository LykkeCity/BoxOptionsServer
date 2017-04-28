using Newtonsoft.Json;

namespace BoxOptions.Public.Models
{
    public class CoefModels
    {
        public class CoefRequestResult
        {
            public Coeff[][] Coeffs { get; set; }
        }

        public class Coeff
        {
            [JsonProperty("hitCoeff")]
            public float HitCoeff { get; set; }
            [JsonProperty("missCoeff")]
            public float MissCoeff { get; set; }
        }
    }
}
