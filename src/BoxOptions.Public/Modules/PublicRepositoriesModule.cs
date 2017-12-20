using Autofac;
using AzureStorage.Tables;
using BoxOptions.AzureRepositories;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Settings;
using BoxOptions.Core;
using BoxOptions.Core.Repositories;
using Common.Log;
using Lykke.SettingsReader;

namespace BoxOptions.Public.Modules
{
    public class PublicRepositoriesModule : Module
    {
        private readonly ILog _log;
        private readonly IReloadingManager<BoxOptionsApiSettings> _settings;

        public PublicRepositoriesModule(IReloadingManager<BoxOptionsApiSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
               .As<ILog>()
               .SingleInstance();
            bool isRelease = true;
#if DEBUG
            isRelease = false;
#endif
            ConfigureRepositories(builder, isRelease);

        }

        private void ConfigureRepositories(ContainerBuilder builder, bool isRelease)
        {
            // Client Logs Repository            
            builder.Register<ILogRepository>(ctx =>
               new LogRepository(AzureTableStorage<LogEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage), "ClientEventLogs", _log)));


            // BoxConfig Repository
            builder.Register<IBoxConfigRepository>(ctx =>
               new BoxConfigRepository(AzureTableStorage<BoxSizeEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage),
                       isRelease ? "BoxConfig" : "DEVBoxConfig",
                       _log)));

            // Activities Repository
            builder.Register<IActivityRepository>(ctx =>
               new ActivityRepository(AzureTableStorage<ActivityEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage),
                       "Activities",
                       _log)));

            // Asset Database Interface
            // and dependecy repositories
            var assetrepo = new AssetRepository(AzureTableStorage<BestBidAskEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage), "BestBidAskHistory", _log));
            builder.RegisterInstance<IAssetDatabase>(new Processors.AzureQuoteDatabase(assetrepo));
            
            // Game Database Interface
            // and dependecy repositories
            var userRepo = new UserRepository(
                   AzureTableStorage<UserEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage), "UserRepo", _log),
                   AzureTableStorage<UserHistoryEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage), "UserHistory", _log));
            var gameRepo = new GameRepository(AzureTableStorage<GameBetEntity>.Create(
                       _settings.Nested(s => s.ConnectionStrings.BoxOptionsApiStorage), "GameRepo", _log));
            builder.RegisterInstance<IGameDatabase>(new Processors.AzureGameDatabase(userRepo, gameRepo));
            

        }
    }
}
