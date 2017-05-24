using BoxOptions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoxOptions.Services
{
    public class GameManager:IGameManager
    {
        public event EventHandler<BoxEventArgs> BetWin;
        public event EventHandler<BoxEventArgs> BetLose;

        /// <summary>
        /// User List
        /// </summary>
        List<Models.User> userList;


        /// <summary>
        ///  Active games: UserId/Active Game
        ///  Only one active game per user at any given time.
        /// </summary>        
        Dictionary<string, Models.Game> activeGames;


        public GameManager()
        {
            userList = new List<Models.User>();
            activeGames = new Dictionary<string, Models.Game>();

        }

        

        public string Launch(string userId)
        {
            Console.WriteLine("{0}> Launch({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            return "OK";
        }
        public string Sleep(string userId)
        {
            Console.WriteLine("{0}> Sleep({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            return "OK";
        }

        public string Wake(string userId)
        {
            Console.WriteLine("{0}> Wake({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            return "OK";
        }

        public string GameStart(string userId, string assetPair)
        {
            Console.WriteLine("{0}> GameStart({1} - {2})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair);
            return "OK";
        }
        public string GameClose(string userId)
        {
            Console.WriteLine("{0}> GameClose({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            return "OK";
        }
        
        public string PlaceBet(string userId, string box, decimal bet)
        {
            Console.WriteLine("{0}> PlaceBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, bet);
            return "OK";
        }

        public string ChangeBet(string userId, string box, decimal betAmount)
        {
            Console.WriteLine("{0}> ChangeBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, betAmount);
            return "OK";
        }

        public string ChangeScale(string userId, decimal scale)
        {
            Console.WriteLine("{0}> Wake({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            return "OK";
        }

        

        


        

        Models.User GetUserById(string userId)
        {
            return (from u in userList
                    where u.UserId == userId
                    select u).FirstOrDefault();
        }

    }
}
