using Autofac;
using BoxOptions.Core;
using BoxOptions.Services;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace BoxOptions.Public.Modules
{
    public class PublicApiModule : Module
    {
        private readonly BoxOptionsSettings _settings;

        public PublicApiModule(BoxOptionsSettings settings)
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

            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterType<MicrographCacheService>()
                .As<IMicrographCacheService>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<PricesWampService>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<RpcMethodsService>()
                .As<IRpcMethodsService>()
                .SingleInstance();
        }
    }
}
