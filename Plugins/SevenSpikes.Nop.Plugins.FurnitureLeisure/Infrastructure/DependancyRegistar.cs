using Autofac;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Data;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Services;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure
{
    public class DependancyRegistar : BaseDependancyRegistrar7Spikes
    {
        public DependancyRegistar() : base(Plugin.SystemName)
        {
        }

        protected override void RegisterPluginObjectContext(ContainerBuilder builder)
        {
            RegisterPluginObjectContext(builder, x => new FurnitureLeisureObjectContext(x));
        }

        protected override void RegisterPluginServices(ContainerBuilder builder)
        {
            builder.RegisterType<EmailHelper>().As<IEmailHelper>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerHelper>().As<ICustomerHelper>().InstancePerLifetimeScope();
            builder.RegisterType<FurnitureLeisureExportManager>().As<IFurnitureLeisureExportManager>().InstancePerLifetimeScope();
            builder.RegisterType<FurnitureLeisureImportManager>().As<IFurnitureLeisureImportManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProductSearchTermService>().As<IProductSearchTermService>().InstancePerLifetimeScope();
        }

        protected override void CreateModelMappings()
        {
            CreateMvcModelMap<FurnitureLeisureSettingsModel, FurnitureLeisureSettings>();
            CreateMvcModelMap<FurnitureLeisureSettings, FurnitureLeisureSettingsModel>();
        }

        protected override void RegisterPluginRepositories(ContainerBuilder builder)
        {
            RegisterPluginRepository<ProductSearchTerms>(builder);
        }
    }
}