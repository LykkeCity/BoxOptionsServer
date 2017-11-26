using Autofac;
using BoxOptions.Common.Settings;

namespace BoxOptions.Public.Modules
{
    public class PublicSettingsModule: Module
    {
        private readonly BoxOptionsApiSettings _settings;

        public PublicSettingsModule(BoxOptionsApiSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings).SingleInstance();
        }
    }
}
