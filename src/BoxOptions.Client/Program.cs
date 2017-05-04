namespace BoxOptions.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Waiting for server to come up");
            System.Threading.Thread.Sleep(5000);
            var client = new MtClient();

            client.Connect(ClientEnv.Local);

            var assets = client.GetAssets();
            var chart = client.GetChardData();
            client.Prices();
            System.Console.ReadLine();
        }
    }
}