using Newtonsoft.Json;

namespace BoxOptions.CoefficientCalculator.Algo
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class BoxOption
    {
        // Transient (ScriptIgnoreAttribute)
        private long _startsInMS;
        private long _lenInMS;
        private double _relatUpStrike;
        private double _relatBotStrike;

        private double _hitCoeff;
        private double _missCoeff;

        public BoxOption(long startsInMS, long lenInMS, double relatUpStrike, double relatBotStrike)
        {
            _startsInMS = startsInMS;
            _lenInMS = lenInMS;
            _relatUpStrike = relatUpStrike;
            _relatBotStrike = relatBotStrike;
        }
        [JsonProperty]
        public double HitCoeff { get => _hitCoeff; }
        [JsonProperty]
        public double MissCoeff { get => _missCoeff; }

        public long StartsInMS { get => _startsInMS; }
        public long LenInMS { get => _lenInMS; }
        public double RelatUpStrike { get => _relatUpStrike; }
        public double RelatBotStrike { get => _relatBotStrike; }

        public BoxOption CloneBoxOption()
        {
            BoxOption boxOption = new BoxOption(_startsInMS, _lenInMS, _relatUpStrike, _relatBotStrike)
            {
                _hitCoeff = this._hitCoeff,
                _missCoeff = this._missCoeff,
                _relatUpStrike = this._relatUpStrike,
                _relatBotStrike = this._relatBotStrike
            };
            return boxOption;
        }

        public void SetCoefficients(double[] coefficients)
        {
            _hitCoeff = coefficients[0];
            _missCoeff = coefficients[1];
        }
    }
}
