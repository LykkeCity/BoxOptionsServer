using BoxOptions.Common.Interfaces;
using BoxOptions.Services.Interfaces;
using BoxOptions.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class GameManager:IGameManager
    {
        public enum GameStatus
        {
            Created = 0,
            Launch = 1,
            Wake = 2,
            Sleep = 3,
            GameStarted = 4,
            GameClosed = 5,
            ChangeBet = 6,
            ChangeScale = 7,
            BetPlaced = 8,
            BetWon = 9,
            BetLost = 10
        }

        public event EventHandler<BoxEventArgs> BetWin;
        public event EventHandler<BoxEventArgs> BetLose;

        
        List<UserState> userList;
        IGameDatabase database;
        ICoefficientCalculator calculator;

        public GameManager(IGameDatabase database, ICoefficientCalculator calculator)
        {            
            this.database = database;
            this.calculator = calculator;
            userList = new List<UserState>();
        }


        private UserState GetUserState(string userId)
        {
            var ulist = from u in userList
                          where u.UserId == userId
                          select u;
            if (ulist.Count() > 1)
                throw new InvalidOperationException("User State List has duplicate entries");

            UserState current = ulist.FirstOrDefault();
            if (current == null)
            {
                // UserState not in current cache,
                // download it from database
                Task<UserState> t = LoadUserStateFromDb(userId);                
                t.Wait();
                current = t.Result;
                
                // add it to cache
                userList.Add(current);
            }

            return current;

        }

        private async Task<UserState> LoadUserStateFromDb(string userId)
        {
            // Database object fetch
            UserState retval = await database.LoadUserState(userId);

            if (retval == null)
            {
                // UserState not in database
                // Create new
                retval = new UserState(userId);

                // Save it to Database
                await database.SaveUserState(retval);
            }
            else
            {
                // Load Current game if CurrentGameId is filled
                if (!string.IsNullOrEmpty(retval.CurrentGameId))
                {
                    Game game = await database.LoadGame(retval.CurrentGameId);
                    retval.SetGame(game);
                }
            }
            
            return retval;
        }
      

        #region IGameManager
        public string Launch(string userId)
        {
            Console.WriteLine("{0}> Launch({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);

            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Launch);
            
            return "OK";
        }
        
        public string Sleep(string userId)
        {
            Console.WriteLine("{0}> Sleep({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Sleep);
            return "OK";
        }

        public string Wake(string userId)
        {
            Console.WriteLine("{0}> Wake({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            UserState userState = GetUserState(userId);
            userState.SetStatus((int)GameStatus.Wake);
            return "OK";
        }

        public string GameStart(string userId, string assetPair)
        {
            Console.WriteLine("{0}> GameStart({1} - {2})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, assetPair);
                        
            try
            {
                // Get user state
                UserState userState = GetUserState(userId);

                // Game ongoing, must be closed first
                if (userState.CurrentGame != null)
                    throw new InvalidOperationException($"this user already has an ongoing game [{userState.CurrentGame.AssetPair}]");

                // Create new game

                Game newgame = new Game(assetPair, Guid.NewGuid().ToString());

                // TODO: set game parameters
                
                // Get coefficients from calculator
                // calculator.ChangeAsync
                // 
                

                // Assign Game to user;
                userState.SetGame(newgame);

                // Save gave to database
                database.SaveGame(newgame);


                // Save userState to database
                database.SaveUserState(userState);

                // Set Status
                userState.SetStatus((int)GameStatus.GameStarted);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }

        public string GameClose(string userId)
        {
            Console.WriteLine("{0}> GameClose({1})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId);
            try
            {
                // Get user state
                UserState userState = GetUserState(userId);

                //Get ongoing gameId
                if (userState.CurrentGame == null)
                    throw new InvalidOperationException("User has no game ongoing.");

                // TODO: Terminate game

                // Remove game from user
                userState.RemoveGame();

                // Set status
                userState.SetStatus((int)GameStatus.GameClosed);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
        public string PlaceBet(string userId, string box, decimal bet)
        {
            Console.WriteLine("{0}> PlaceBet({1} - {2} - {3:f16})", DateTime.UtcNow.ToString("HH:mm:ss.fff"), userId, box, bet);
            try
            {
                // Get user state
                UserState userState = GetUserState(userId);

                //Get ongoing gameId
                if (userState.CurrentGame == null)
                    throw new InvalidOperationException("User has no game ongoing.");

                // Validate balance
                if (bet > userState.Balance)
                    throw new InvalidOperationException("User has no balance for the bet.");

                // TODO: Get Box from... somewhere
                Box boxObject = new Box(); // TODO: Get the Box object from received string

                // TODO: Place Bet
                userState.CurrentGame.PlaceBet(boxObject, bet);
                                
                // Set status
                userState.SetStatus((int)GameStatus.BetPlaced);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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
        #endregion
        
    }
}
