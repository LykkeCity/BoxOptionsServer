namespace BoxOptions.CoefficientCalculator.Algo
{
    internal class IE
    {
        private int _type; // is a type of the IE: +1 or -1 for DC IEs, +2 and -2 for an overshoot IE
        private long _time; // is when the IE happened
        private double _level; // is the price level at which the IE happened
        private double _osL; // is overshoot length, in fraction of the previous DC price
        private double _sqrtOsDeviation; // is the squared overshoot deviation, (w(d) - d)^2

        public IE(int type, long time, double level, double osL, double sqrtOsDeviation)
        {
            _type = type;
            _time = time;
            _level = level;
            _osL = osL;
            _sqrtOsDeviation = sqrtOsDeviation;
        }

        public int Type { get => _type;}
        public long Time { get => _time; set => _time = value; }
        public double Level { get => _level; }
        public double OsL { get => _osL; }
        public double SqrtOsDeviation { get => _sqrtOsDeviation; }
    }
}
