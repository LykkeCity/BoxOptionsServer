using System;
using Autofac;
using AzureStorage.Tables;
using BoxOptions.AzureRepositories;
using BoxOptions.Common;
using BoxOptions.Core;
using BoxOptions.Services;
using Common.Log;
using Lykke.Logs;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.V2;
using WampSharp.V2.Realm;
using BoxOptions.Core.Interfaces;
using BoxOptions.Common.Interfaces;
using Lykke.SettingsReader;
using BoxOptions.Common.Settings;

namespace BoxOptions.Public.Modules
{
    public class PublicApiModule : Module
    {        
        private readonly IReloadingManager<BoxOptionsApiSettings> _settings;
        
        public PublicApiModule(IReloadingManager<BoxOptionsApiSettings> settings)
        {
            _settings = settings;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            var host = new WampHost();
            var realm = host.RealmContainer.GetRealmByName("box-options");

            builder.RegisterInstance(host)
            .As<IWampHost>()
            .SingleInstance();

            builder.RegisterInstance(realm)
                .As<IWampHostedRealm>()
                .SingleInstance();

            ConfigureServices(builder);

        }
       
        private void ConfigureServices(ContainerBuilder builder)
        {
           
            builder.RegisterType<AssetQuoteSubscriber>()
                .As<IAssetQuoteSubscriber>()
                .As<IStartable>()
                .SingleInstance();

            // History Holder
            builder.RegisterType<HistoryHolder>()
              .As<IHistoryHolder>()
              .As<IStartable>()
              .SingleInstance();

            // Micrograph Cache Service
            builder.RegisterType<MicrographCacheService>()
              .As<IMicrographCache>()
              .As<IStartable>()
              .SingleInstance();

            // Price Feed Publisher Service
            builder.RegisterType<PriceFeedService>()
                .As<IStartable>()
                .SingleInstance();

            // Wamp Rpc Service
            builder.RegisterType<WampRpcService>()
                .As<IRpcMethods>()
                .SingleInstance();

            // Coefficient Calculator Interface
            ICoefficientCalculator coefCalculator;
            if (_settings.CurrentValue.CoefApiUrl.ToLower() == "mock")
                coefCalculator = new Processors.MockCoefficientCalculator();
            else
                coefCalculator = new Processors.ProxyCoefficientCalculator(_settings.CurrentValue);
            builder.RegisterInstance(coefCalculator)
                .As<ICoefficientCalculator>()
                .SingleInstance();
                       
            // Game Manager Interface
            builder.RegisterType<GameManager>()
              .As<IGameManager>()
              .SingleInstance();
        }
    }
}
