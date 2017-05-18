using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public
{
    public enum BoxOptionEvent
    {
        BOEventLaunch = 1,
        BOEventWake = 2,
        BOEventSleep = 3,
        BOEventGameStarted = 4,
        BOEventGameClosed = 5,
        BOEventChangeBet = 6,
        BOEventChangeScale = 7,
        BOEventBetPlaced = 8,
        BOEventBetWon = 9,
        BOEventBetLost = 10,
        BOEventCoefRequest = 11,
        BOEventCoefChange = 12
    }
}
