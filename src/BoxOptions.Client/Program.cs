namespace BoxOptions.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MtClient();

            client.Connect(ClientEnv.Prod);

            //client.GetAssets();
            //client.GetChardData();
            //client.Prices();
        }
    }
}