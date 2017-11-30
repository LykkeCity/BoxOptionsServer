namespace BoxOptions.Common.Settings
{
    public class BoxOptionsApiSettings
    {
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new ConnectionStringsSettings();
        public PricesSettingsBoxOptions PricesSettingsBoxOptions { get; set; }
        public GameManagerSettings GameManager { get; set; } = new GameManagerSettings();
        public HistoryHolderSettings HistoryHolder { get; set; } = new HistoryHolderSettings();
        public CoefficientCalculatorSettings CoefficientCalculator { get; set; }
        public string CoefApiUrl { get; set; }
    }
}
