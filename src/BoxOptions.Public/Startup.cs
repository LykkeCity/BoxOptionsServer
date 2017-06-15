using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BoxOptions.Common;
using BoxOptions.Core;
using BoxOptions.Public.Exceptions;
using BoxOptions.Public.Modules;
using BoxOptions.Services;
using Common.Log;
using Flurl.Http;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using WampSharp.AspNetCore.WebSockets.Server;
using WampSharp.Binding;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace BoxOptions.Public
{
    public class Startup
    {
        public IConfigurationBuilder Builder { get; }
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.dev.json", true, true)
                .AddEnvironmentVariables();

            Configuration = Builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            BoxOptionsSettings settings = BuildConfiguration(services);

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
                                               
            //var settings = Environment.IsEnvironment("Development")
            //    ? Configuration.Get<BoxOptionsSettings>()
            //    : Configuration.GetValue<string>("SettingsUrl").GetJsonAsync<BoxOptionsSettings>().Result;

            var builder = new ContainerBuilder();
                        
            builder.RegisterModule(new PublicApiModule(services, settings, Program.Name));

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }
        private BoxOptionsSettings BuildConfiguration(IServiceCollection services)
        {
            BoxOptionsSettings settings = null;
            if (Environment.IsEnvironment("Development"))
                settings = Lykke.SettingsReader.SettingsReader.ReadGeneralSettings<BoxOptionsSettings>("appsettings.dev.json");
            else
                settings = Lykke.SettingsReader.SettingsReader.ReadGeneralSettings<BoxOptionsSettings>(new Uri(Configuration.GetValue<string>("SettingsUrl")));

            return settings;
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
                    new JTokenJsonBinding(),
                    new JTokenMsgpackBinding());
            });

            host.Open();

            appLog.WriteInfoAsync("BoxOptionsServer", "Startup.Configure", null, string.Format("Lykke.BoxOptionsServer [{0}] started.", Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion));


        }
    }
}
