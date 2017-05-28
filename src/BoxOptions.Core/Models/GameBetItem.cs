using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Core.Models
{
    public class GameBetItem : IGameBetItem
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public DateTime Date { get; set; }
        public string Box { get; set; }
        public string BetAmount { get; set; }
        public string Parameters { get; set; }
        //IUserParameterItem Parameters{ get; set; }
    }
}
