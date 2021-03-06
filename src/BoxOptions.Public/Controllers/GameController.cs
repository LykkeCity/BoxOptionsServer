﻿using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Core.Repositories;
using BoxOptions.Public.Models;
using BoxOptions.Public.ViewModels;
using Common.Log;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class GameController : Controller
    {
        private readonly BoxOptionsApiSettings _settings;
        private readonly IGameManager gameManager;
        private readonly IAssetQuoteSubscriber assetQuoteSubscriber;
        private readonly ILog log;
        private readonly IBoxConfigRepository assetConfigRepo;

        public GameController(BoxOptionsApiSettings settings, IGameManager gameManager, IAssetQuoteSubscriber assetQuoteSubscriber, ILog log, IBoxConfigRepository assetConfigRepo)
        {
            _settings = settings;
            this.gameManager = gameManager;
            this.assetQuoteSubscriber = assetQuoteSubscriber;
            this.log = log;
            this.assetConfigRepo = assetConfigRepo;
        }
        //[HttpGet]
        //[Route("setassetdefaultconfig")]
        //public async Task<IActionResult> SetAssetDefaultConfig()
        //{
        //    List<string> assets = new List<string>();
        //    assets.AddRange(_settings.BoxOptionsApi.PricesSettingsBoxOptions.PrimaryFeed.AllowedAssets);
        //    assets.AddRange(_settings.BoxOptionsApi.PricesSettingsBoxOptions.SecondaryFeed.AllowedAssets);

        //    List<string> distict = assets.Distinct().ToList();
        //    List<BoxSize> defaultcfg = new List<BoxSize>();
        //    foreach (var asset in distict)
        //    {
        //        defaultcfg.Add(new BoxSize()
        //        {
        //            AssetPair = asset,
        //            BoxesPerRow = 7,
        //            BoxHeight = 7000,
        //            BoxWidth = 0.00003,
        //            TimeToFirstBox = 4000,
        //            SaveHistory = true,
        //            GameAllowed = true
        //        });

        //    }

        //    gameManager.SetBoxConfig(defaultcfg.ToArray());

        //    await log?.WriteInfoAsync("BoxOptions.Public.GameController", "SetAssetDefaultConfig", null, string.Format("Box configuration set to defaults for assets {0}", string.Join(",", distict)));

        //    return Ok();
        //}

        [HttpGet]
        [Route("assetconfiguration")]
        public async Task<IActionResult> AssetConfiguration()
        {

            var boxConfig = await assetConfigRepo.GetAll();
            var Volats = gameManager.GetVolatilities();


            var viewModel = new AssetConfigurationViewModel
            {
                SaveInformation = "Saving ths form will update Asset Configuration and recalculate game parameters.",
                BoxConfiguration = boxConfig.Select(i =>
                                    new BoxSizeModel
                                    {
                                        AssetPair = i.AssetPair,
                                        BoxesPerRow = i.BoxesPerRow,
                                        BoxHeight = i.BoxHeight,
                                        BoxWidth = i.BoxWidth,
                                        GameAllowed = i.GameAllowed,
                                        SaveHistory = i.SaveHistory,
                                        TimeToFirstBox = i.TimeToFirstBox,
                                        ScaleK = i.ScaleK,
                                        Volatility = Volats.ContainsKey(i.AssetPair) ? Volats[i.AssetPair] : 0
                                    }).ToList()
            };
            
            return View(viewModel);
        }
        [HttpPost]
        [Route("assetconfiguration")]
        public async Task<ActionResult> AssetConfiguration([Bind("BoxConfiguration")] AssetConfigurationViewModel config)
        {
            if (ModelState.IsValid)
            {

                await assetConfigRepo.InsertManyAsync(config.BoxConfiguration);
                config.SaveInformation = "Asset Configuration Saved Successfully";
                await gameManager.ReloadGameAssets();
                bool asOk = await assetQuoteSubscriber.ReloadAssetConfiguration();

                await log?.WriteInfoAsync("BoxOptions.Public.GameController", "AssetConfiguration", null, "Asset configuration changed");
            }

            return View(config);
        }
    }
}
