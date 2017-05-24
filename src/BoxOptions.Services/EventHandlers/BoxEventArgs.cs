using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services
{
    public class BoxEventArgs:EventArgs
    {
        string gameId;
        Models.Box box;

        public BoxEventArgs(string gameId, Models.Box box)
        {
            this.gameId = gameId;
            this.box = box;
        }

        public string GameId { get => gameId; }
        public Models.Box Box { get => box; }

    }
}
