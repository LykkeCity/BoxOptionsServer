namespace BoxOptions.Common.Settings
{
    public class CoefficientCalculatorInstrumentSettings
    {
        public string Name { get; set; }
        public long Period { get; set; }
        public long TimeToFirstOption { get; set; }
        public long OptionLen { get; set; }

        public double PriceSize { get; set; }
        public int NPriceIndex { get; set; }
        public int NTimeIndex { get; set; }
        public double MarginHit { get; set; }
        public double MarginMiss { get; set; }
        public double MaxPayoutCoeff { get; set; }
        public double BookingFee { get; set; }

        public bool HasWeekend { get; set; }

        public double Delta { get; set; }
        public long MovingWindow { get; set; }

        public string ActivityFileName { get; set; }

        public int SmileVar { get; set; }
    }
}
