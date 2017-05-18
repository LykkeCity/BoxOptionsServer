using System;

namespace BoxOptions.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect to WAMP: prod[P] or dev[D] connection.");
            string input = Console.ReadLine();
            ClientEnv env = ClientEnv.Local;
            if (input.ToLower() == "p")
                env = ClientEnv.Prod;

            var client = new MtClient();
            client.Connect(env);

            var assets = client.GetAssets();
            var chart = client.GetChardData();
            System.Console.WriteLine("Chart Entries: {0}", chart.Count);

            client.Prices();

            
            do
            {
                input = Console.ReadLine();
                switch (input)
                {
                    case "graph":
                        chart = client.GetChardData();
                        int i = 0;
                        foreach (var item in chart)
                        {
                            Console.WriteLine("{0}>{1}", ++i, item.Key);
                            foreach (var bid in item.Value)
                            {
                                Console.WriteLine("\t{0} > {1}/{2}", bid.Date.ToString("yyyy-MM-dd HH:mm:ss"), bid.Bid, bid.Ask);
                            }
                        }
                        break;
                    case "log":
                        client.PostLog();
                        break;
                    default:
                        break;
                }
            } while (input != "exit");
            client.Stop();

        }
    }
}