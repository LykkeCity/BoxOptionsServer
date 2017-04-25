using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BoxOptions.Core;
using BoxOptions.Public.Modules;
using BoxOptions.Services;
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
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.dev.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
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

            var settings = new BoxOptionsSettings();
            Configuration.Bind(settings);

            builder.RegisterModule(new PublicApiModule(settings));

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            IWampHost host = ApplicationContainer.Resolve<IWampHost>();
            IWampHostedRealm realm = ApplicationContainer.Resolve<IWampHostedRealm>();
            var rpcMethods = ApplicationContainer.Resolve<IRpcMethodsService>();
            

            appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());

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
        }
    }
}
