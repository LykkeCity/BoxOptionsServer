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

            builder.RegisterInstance(host)
                .As<IWampHost>()
                .SingleInstance();

            builder.RegisterInstance(realm)
                .As<IWampHostedRealm>()
                .SingleInstance();

            builder.RegisterInstance(_settings)
                .AsSelf()
                .SingleInstance();

            

            builder.RegisterType<AssetQuoteSubscriber>()
                .As<IAssetQuoteSubscriber>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<MicrographCacheService>()
              .As<IMicrographCache>()
              .As<IStartable>()
              .SingleInstance();

            builder.RegisterType<GameManager>()
              .As<Services.Interfaces.IGameManager>()              
              .SingleInstance();

            builder.RegisterType<PriceFeedService>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<WampRpcService>()
                .As<IRpcMethods>()
                .SingleInstance();


            var logAggregate = new LogAggregate();

            var log = logAggregate.CreateLogger();
            var slackSender = _services.UseSlackNotificationsSenderViaAzureQueue(_settings.SlackNotifications.AzureQueue, log);
            var azureLog = new LykkeLogToAzureStorage(_appName,
                new AzureTableStorage<Lykke.Logs.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.LogsConnString, _appName.Replace(".", string.Empty) + "Logs", log),
                slackSender);
            logAggregate.AddLogger(azureLog);
            log = logAggregate.CreateLogger();

            builder.RegisterInstance(log).As<ILog>();
            builder.RegisterInstance(new LogRepository(new AzureTableStorage<AzureRepositories.LogEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage, 
                "ClientEventLogs", log)))
                .As<ILogRepository>();
            builder.RegisterInstance(new AssetRepository(new AzureTableStorage<AzureRepositories.AssetEntity>(_settings.BoxOptionsApi.ConnectionStrings.BoxOptionsApiStorage,
                "QuoteFeed", log)))
                .As<IAssetRepository>();


            // TODO: Change to Azure Storage in prod env
#if DEBUG
            builder.RegisterType<LocalFSHistory>()
                .As<IBoxOptionsHistory>()
                .SingleInstance();
#else
            builder.RegisterType<Processors.AzureQuoteFeed>()
                .As<IBoxOptionsHistory>()
                .SingleInstance();

#endif


        }
    }
}
