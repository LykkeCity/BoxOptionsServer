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

namespace BoxOptions.Public.Modules
{
    public class PublicApiModule : Module
    {
        private readonly IServiceCollection _services;
        private readonly BoxOptionsSettings _settings;
        private readonly string _appName;
                

        public PublicApiModule(IServiceCollection services, BoxOptionsSettings settings, string appName)
        {
            _services = services;
            _settings = settings;
            _appName = appName;
        }



        protected override void Load(ContainerBuilder builder)
        {
            var host = new WampHost();
            var realm = host.RealmContainer.GetRealmByName("box-options");

            builder.RegisterInstance(_settings)
                .AsSelf()
                .SingleInstance();



#if DEBUG
            var logAggregate = new LogAggregate();
            var log = logAggregate.CreateLogger();
            var slackSender = _services.UseSlackNotificationsSenderViaAzureQueue(_settings.SlackNotifications.AzureQueue, log);
            var azureLog = new LykkeLogToAzureStorage(_appName,
                new AzureTableStorage<Lykke.Logs.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.LogsConnString, "DEV" +_appName.Replace(".", string.Empty) + "Logs", log),
                slackSender);
            logAggregate.AddLogger(azureLog);
            log = logAggregate.CreateLogger();
            builder.RegisterInstance(log).As<ILog>();


            // Client Logs Repository
            builder.RegisterInstance(new LogRepository(new AzureTableStorage<AzureRepositories.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "ClientEventLogs", log)))
                .As<ILogRepository>();

            // User Data Repository
            builder.RegisterInstance(new UserRepository(
                new AzureTableStorage<AzureRepositories.UserEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "UserRepo", log),
                new AzureTableStorage<AzureRepositories.UserHistoryEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "UserHistory", log)))
                .As<IUserRepository>();

            // Game Manager Repository
            builder.RegisterInstance(new GameRepository(
                new AzureTableStorage<AzureRepositories.GameBetEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "GameRepo", log)))
                .As<IGameRepository>();

            // BoxConfig Repository
            builder.RegisterInstance(new BoxConfigRepository(
                new AzureTableStorage<AzureRepositories.BoxSizeEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "DEVBoxConfig", log)))
                .As<IBoxConfigRepository>();

            //// Local File System Asset Database
            //builder.RegisterType<LocalFSHistory>()
            //    .As<IAssetDatabase>()
            //    .SingleInstance();
            
            // Quote Feed Repository
            builder.RegisterInstance(new AssetRepository(new AzureTableStorage<AzureRepositories.BestBidAskEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "BestBidAskHistory", log)))
                .As<IAssetRepository>();

            // Azure Storage Asset Database
            builder.RegisterType<Processors.AzureQuoteDatabase>()
                .As<IAssetDatabase>()
                .SingleInstance();
#else

            var logAggregate = new LogAggregate();
            var log = logAggregate.CreateLogger();
            var slackSender = _services.UseSlackNotificationsSenderViaAzureQueue(_settings.SlackNotifications.AzureQueue, log);
            var azureLog = new LykkeLogToAzureStorage(_appName,
                new AzureTableStorage<Lykke.Logs.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.LogsConnString, _appName.Replace(".", string.Empty) + "Logs", log),
                slackSender);
            logAggregate.AddLogger(azureLog);
            log = logAggregate.CreateLogger();
            builder.RegisterInstance(log).As<ILog>();

            // Client Logs Repository
            builder.RegisterInstance(new LogRepository(new AzureTableStorage<AzureRepositories.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "ClientEventLogs", log)))
                .As<ILogRepository>();

            // Quote Feed Repository
            builder.RegisterInstance(new AssetRepository(new AzureTableStorage<AzureRepositories.BestBidAskEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "BestBidAskHistory", log)))
                .As<IAssetRepository>();

            // User Data Repository
            builder.RegisterInstance(new UserRepository(
                new AzureTableStorage<AzureRepositories.UserEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "UserRepo", log),
                new AzureTableStorage<AzureRepositories.UserHistoryEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "UserHistory", log)))
                .As<IUserRepository>();

            // Game Manager Repository
            builder.RegisterInstance(new GameRepository(
                new AzureTableStorage<AzureRepositories.GameBetEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "GameRepo", log)))
                .As<IGameRepository>();

            // BoxConfig Repository
            builder.RegisterInstance(new BoxConfigRepository(
                new AzureTableStorage<AzureRepositories.BoxSizeEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "BoxConfig", log)))
                .As<IBoxConfigRepository>();

            // Azure Storage Asset Database
            builder.RegisterType<Processors.AzureQuoteDatabase>()
                .As<IAssetDatabase>()
                .SingleInstance();
#endif


            builder.RegisterInstance(host)
              .As<IWampHost>()
              .SingleInstance();

            builder.RegisterInstance(realm)
                .As<IWampHostedRealm>()
                .SingleInstance();

            builder.RegisterType<AssetQuoteSubscriber>()
                .As<IAssetQuoteSubscriber>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<MicrographCacheService>()
              .As<IMicrographCache>()
              .As<IStartable>()
              .SingleInstance();

            builder.RegisterType<PriceFeedService>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<WampRpcService>()
                .As<IRpcMethods>()
                .SingleInstance();

            // Coefficient Calculator Interface
            ICoefficientCalculator coefCalculator;
            if (_settings.BoxOptionsApi.CoefApiUrl.ToLower() == "mock")
                coefCalculator = new Processors.MockCoefficientCalculator();
            else
                coefCalculator = new Processors.ProxyCoefficientCalculator(_settings);
            builder.RegisterInstance(coefCalculator)
                .As<ICoefficientCalculator>()
                .SingleInstance();

            // Game Database Interface
            builder.RegisterType<Processors.AzureGameDatabase>()
              .As<Services.Interfaces.IGameDatabase>()
              .SingleInstance();


            // Game Manager Interface
            builder.RegisterType<GameManager>()
              .As<Services.Interfaces.IGameManager>()
              .SingleInstance();

            // History Holder
            builder.RegisterType<HistoryHolder>()
              .As<Services.Interfaces.IHistoryHolder>()
              .As<IStartable>()
              .SingleInstance();

        }
    }
}
