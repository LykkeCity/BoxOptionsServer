namespace BoxOptions.CoefficientCalculator.Algo
{
    public class BoxOption
    {
        private long startsInMS;
        private long lenInMS;
        private double relatUpStrike;
        private double relatBotStrike;
        private double hitCoeff;
        private double missCoeff;

        public double RelatUpStrike { get => relatUpStrike; }
        public double RelatBotStrike { get => relatBotStrike; }
        public long StartsInMS { get => startsInMS; }
        public long LenInMS { get => lenInMS; }

        public BoxOption(long startsInMS, long lenInMS, double relatUpStrike, double relatBotStrike)
        {
            this.startsInMS = startsInMS;
            this.lenInMS = lenInMS;
            this.relatUpStrike = relatUpStrike;
            this.relatBotStrike = relatBotStrike;
        }

        public BoxOption CloneBoxOption()
        {
            BoxOption boxOption = new BoxOption(startsInMS, lenInMS, relatUpStrike, relatBotStrike);
            boxOption.hitCoeff = this.hitCoeff;
            boxOption.missCoeff = this.missCoeff;
            boxOption.relatUpStrike = this.relatUpStrike;
            boxOption.relatBotStrike = this.relatBotStrike;
            return boxOption;
        }

        public void SetCoefficients(double[] coefficients)
        {
            this.hitCoeff = coefficients[0];
            this.missCoeff = coefficients[1];

        }

        public double GetHitCoeff()
        {
            return hitCoeff;
        }

        public double GetMissCoeff()
        {
            return missCoeff;
        }

    }
}
