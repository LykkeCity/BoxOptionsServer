using System.Collections.Generic;
using BoxOptions.Core;
using WampSharp.V2.Rpc;

namespace BoxOptions.Services
{
    public interface IRpcMethods
    {
        /// <summary>
        /// Client calls init.chartdata RPC to get that data for charts
        /// </summary>
        /// <returns></returns>
        [WampProcedure("init.chartdata")]
        Dictionary<string, Price[]> InitChartData();

        /// <summary>
        /// Client calls init.assets RPC method to get list of asset pairs
        /// </summary>
        /// <returns></returns>
        [WampProcedure("init.assets")]
        AssetPair[] InitAssets();
                
        [WampProcedure("game.launch")]
        string Launch(string userId);
                
        [WampProcedure("game.wake")]
        string Wake(string userId);

        [WampProcedure("game.sleep")]
        string Sleep(string userId);

        [WampProcedure("game.start")]
        string GameStart(string userId, string assetPair);

        [WampProcedure("game.close")]
        string GameClose(string userId);

        [WampProcedure("game.placebet")]
        string PlaceBet(string userId, string box, decimal betAmount);

        [WampProcedure("game.changebet")]
        string ChangeBet(string userId, string box, decimal betAmount);

        [WampProcedure("game.changescale")]
        string ChangeScale(string userId, decimal scale);
    }
}
