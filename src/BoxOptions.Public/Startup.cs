using Autofac;
using Autofac.Extensions.DependencyInjection;
using BoxOptions.Common;
using BoxOptions.Common.Extensions;
using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Services;
using BoxOptions.Common.Settings;
using BoxOptions.Public.Exceptions;
using BoxOptions.Public.Modules;
using BoxOptions.Services;
using Common.Log;
using Lykke.AzureQueueIntegration;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using System;
using WampSharp.AspNetCore.WebSockets.Server;
using WampSharp.Binding;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace BoxOptions.Public
{
    public class Startup
    {
        //public IConfigurationBuilder Builder { get; }
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddDevJson(env)
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
               .AddConsole(LogLevel.Error)
               .AddDebug(LogLevel.Error);
                        
            services.AddSingleton(loggerFactory);
            services.AddLogging();
            services.AddMvc(o =>
                {
                    o.Filters.Add(new HandleAllExceptionsFilterFactory());
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Api"
                });
                options.DescribeAllEnumsAsStrings();
            });
                                                
            var builder = new ContainerBuilder();

            var settings = Configuration.LoadSettings<AppSettings>();
            var boSettings = settings.Nested(s => s.BoxOptionsApi);

            SetupLoggers(services, settings, boSettings);

            RegisterModules(builder, boSettings);
            
            builder.Populate(services);
            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
        }
                
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILog appLog, IApplicationLifetime appLifetime)
        {
            IWampHost host = ApplicationContainer.Resolve<IWampHost>();
            IWampHostedRealm realm = ApplicationContainer.Resolve<IWampHostedRealm>();
            var rpcMethods = ApplicationContainer.Resolve<IRpcMethods>();


            appLifetime.ApplicationStopped.Register(() =>
            {                
                ApplicationContainer.Dispose();                
            });

            appLifetime.ApplicationStarted.Register(() =>
                realm.Services.RegisterCallee(rpcMethods).Wait()
            );

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi("swagger/ui/index");

            app.Map("/ws", builder =>
            {
                builder.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(1) });

                host.RegisterTransport(new AspNetCoreWebSocketTransport(builder),
                    new JTokenJsonBinding());
            });

            host.Open();

            appLog.WriteInfoAsync("BoxOptionsServer", "Startup.Configure", null, string.Format("Lykke.BoxOptionsServer [{0}] started.", Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion));


        }
              
        private static void SetupLoggers(IServiceCollection services, IReloadingManager<AppSettings> settings, IReloadingManager<BoxOptionsApiSettings> boxOptionsSettings)
        {
            var consoleLogger = new LogToConsole();

            var azureQueue = new AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            };

            var commonSlackService =
                services.UseSlackNotificationsSenderViaAzureQueue(azureQueue, consoleLogger);

            var log = services.UseLogToAzureStorage(boxOptionsSettings.Nested(s => s.ConnectionStrings.LogsConnString), commonSlackService,
               "BoxOptionsPublicLogs", consoleLogger);

            LogLocator.CommonLog = log; 
        }

        private void RegisterModules(ContainerBuilder builder, IReloadingManager<BoxOptionsApiSettings> settings)
        {
            builder.RegisterModule(new PublicSettingsModule(settings.CurrentValue));
            builder.RegisterModule(new PublicRepositoriesModule(settings, LogLocator.CommonLog));
            builder.RegisterModule(new PublicApiModule(settings));
        }
    }
}
