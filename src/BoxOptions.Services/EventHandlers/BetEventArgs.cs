using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services
{
    public class BetEventArgs:EventArgs
    {        
        readonly Models.GameBet bet;

        public BetEventArgs(Models.GameBet bet)
        {
            this.bet = bet;
        }
        public Models.GameBet Bet { get => bet; }

    }
}
