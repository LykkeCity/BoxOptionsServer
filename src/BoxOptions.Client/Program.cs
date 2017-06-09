using System;

namespace BoxOptions.Client
{
    class Program
    {
        const string UserId1 = "2E95B3E8-C9B3-4D66-830E-11922DD21EC0";
        const string UserId2 = "404af161-50c5-477b-8375-89bfc7150001";
        const string UserId3 = "604af161-50c5-477b-8375-89bfc7150001";

        internal static bool ShowFeed;
        internal static string UserId;
        private static System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-us");

        static void Main(string[] args)
        {

            ShowFeed = true;
            UserId = UserId1;

            Console.WriteLine("Connect to WAMP: prod[P], dev[D] or local[L] connection.");
            string input = Console.ReadLine();
            ClientEnv env = ClientEnv.Local;
            if (input.ToLower() == "p")
                env = ClientEnv.Prod;
            else if (input.ToLower() == "d")
                env = ClientEnv.Dev;

            var client = new MtClient();
            client.Connect(env);

            var assets = client.GetAssets();
            var chart = client.GetChardData();
            System.Console.WriteLine("Chart Entries: {0}", chart.Count);

            string inituser = client.InitUser(UserId);
            System.Console.WriteLine("Init User: {0}", inituser);

            client.Prices();
            client.SubscribeGameEvents();



            do
            {
                input = Console.ReadLine();
                switch (input)
                {
                    default:
                        Console.WriteLine("Unknown command: [{0}]", input);
                        break;
                    case "help":
                        Console.WriteLine(" <===HELP===>");
                        Console.WriteLine(" > help - show this help");
                        Console.WriteLine(" > user - change user");
                        Console.WriteLine(" > feed - toggles feed output to console.");
                        Console.WriteLine(" > graph - micrograph cached data");
                        Console.WriteLine(" > log - perform log event");                        
                        Console.WriteLine(" > placebet - place a new bet on a box");
                        Console.WriteLine(" > placebet - place multiple bets at once");
                        Console.WriteLine(" > getbalance - gets user balance");
                        Console.WriteLine(" > setbalance - sets user balance");                        
                        Console.WriteLine(" > coeff - gets asset coefficients");

                        break;
                    case "user":
                        Console.Write("\tUser Number 1/2/3>");
                        string s_user = Console.ReadLine();
                        if (s_user == "2")
                            UserId = UserId2;
                        else if (s_user == "3")
                            UserId = UserId3;
                        else
                            UserId = UserId1;
                        string initUserRes = client.InitUser(UserId);
                        client.SubscribeGameEvents();
                        Console.WriteLine("User set to: [{0}] Answer:{1}", UserId, initUserRes);
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
                    case "placebet":
                        string pb_box = "{"+
                            "\"Id\": \"james@example.com\"," +
                            "\"MinPrice\": 0.0001," +
                            "\"MaxPrice\": 0.0009," +
                            "\"TimeToGraph\": 7.377049180327869," +
                            "\"TimeLength\": 7.9999999," +
                            "\"Coefficient\": 0.987654321" +
                            "}";

                        Console.Write("\tAssetPair>");
                        string s_pair = Console.ReadLine();
                        Console.Write("\tBet>");
                        string pb_bet = Console.ReadLine();
                        decimal pb_bet_val = 0;
                        decimal.TryParse(pb_bet, System.Globalization.NumberStyles.AllowDecimalPoint, CI, out pb_bet_val);
                        if (pb_bet_val > 0)
                            client.PlaceBet(UserId, s_pair, pb_box, pb_bet_val);
                        else
                            Console.WriteLine("Invalid Bet Value");
                        break;
                    case "placebets":
                        client.PlaceBets(UserId);
                        break;
                    case "getbalance":
                        client.GetBalance(UserId);
                        break;
                    case "setbalance":
                        Console.Write("\tNew Balance>");
                        string newbal = Console.ReadLine();
                        decimal newbal_val = 0;
                        decimal.TryParse(newbal, System.Globalization.NumberStyles.AllowDecimalPoint, CI, out newbal_val);
                        if (newbal_val >= 0)
                            client.SetBalance(UserId, newbal_val);
                        break;
                    
                    case "coeff":
                        Console.Write("\tAsset>");
                        string co_asset = Console.ReadLine();
                        client.RequestCoeff("2E95B3E8-C9B3-4D66-830E-11922DD21EC0", co_asset);
                        break;
                }
            } while (input != "exit");
            client.Stop();

        }
    }
}