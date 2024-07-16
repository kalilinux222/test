using Autofac;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using SevenSpikes.Nop.Plugins.JsonLD.Helpers;
using SevenSpikes.Nop.Plugins.JsonLD.Models.Admin;

namespace SevenSpikes.Nop.Plugins.JsonLD.Infrastructure
{
    public class DependancyRegistar : BaseDependancyRegistrar7Spikes
    {
        protected override void CreateModelMappings()
        {
            CreateMvcModelMap<JsonLdAdminModel, JsonLdSettings>();
            CreateMvcModelMap<JsonLdSettings, JsonLdAdminModel>();
        }

        protected override void RegisterPluginServices(ContainerBuilder builder)
        {
            builder.RegisterType<DictionaryHelper>().As<IDictionaryHelper>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationDataHelper>().As<IOrganizationDataHelper>().InstancePerLifetimeScope();
            builder.RegisterType<ProductCustomPropertiesHelper>().As<IProductCustomPropertiesHelper>().InstancePerLifetimeScope();
            builder.RegisterType<ProductDataHelper>().As<IProductDataHelper>().InstancePerLifetimeScope();
            builder.RegisterType<BreadcrumbHelper>().As<IBreadcrumbHelper>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryDataHelper>().As<ICategoryDataHelper>().InstancePerLifetimeScope();
        }
    }
}
