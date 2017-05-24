using System;

namespace BoxOptions.Client
{
    class Program
    {
        const string UserId = "204af161-50c5-477b-8375-89bfc715c2cc";
        internal static bool ShowFeed;
        static void Main(string[] args)
        {

            ShowFeed = true;

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
                    case "help":
                        Console.WriteLine(" <===HELP===>");
                        Console.WriteLine(" > help - show this help");
                        Console.WriteLine(" > feed - toggles feed output to console.");
                        Console.WriteLine(" > graph - micrograph cached data");
                        Console.WriteLine(" > log - perform log event");
                        Console.WriteLine(" > launch - launch new game");
                        Console.WriteLine(" > wake - game.wake");
                        Console.WriteLine(" > start");
                        break;
                    case "feed":
                        ShowFeed = !ShowFeed;
                        if (ShowFeed)
                            Console.WriteLine("Feed output is ON");
                        else
                            Console.WriteLine("Feed output is OFF");
                        break;
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
                    case "launch":
                        client.Launch(UserId);
                        break;
                    case "wake":
                        client.Wake(UserId);
                        break;
                    case "sleep":
                        client.Wake(UserId);
                        break;
                    case "start":
                        Console.Write("\tAssetPair>");
                        string s_pair = Console.ReadLine();
                        client.GameStart(UserId, s_pair);
                        break;
                }
            } while (input != "exit");
            client.Stop();

        }
    }
}