using Autofac;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure
{
    public class DependancyRegistar : BaseDependancyRegistrar7Spikes
    {
        protected override void RegisterPluginServices(ContainerBuilder builder)
        {
            builder.RegisterType<AttributeHelper>().As<IAttributeHelper>().InstancePerDependency();

            builder.RegisterType<PreviewImageBuilderHelper>().As<IPreviewImageBuilderHelper>().InstancePerDependency();
        }
    }
}