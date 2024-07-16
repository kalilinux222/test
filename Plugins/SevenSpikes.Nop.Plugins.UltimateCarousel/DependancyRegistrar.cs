using Autofac;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Data;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Services;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel
{
    public class DependancyRegistrar : BaseDependancyRegistrar7Spikes
    {
        public DependancyRegistrar()
            : base(Constants.UltimateCarouselContextName)
        {
        }

        protected override void RegisterPluginObjectContext(ContainerBuilder builder)
        {
            RegisterPluginObjectContext(builder, x => new UltimateCarouselObjectContext(x));
        }

        protected override void CreateModelMappings()
        {
            CreateMvcModelMap<CarouselModel, UCarousel>();
            CreateMvcModelMap<CarouselItemModel, CarouselItem>();
            CreateMvcModelMap<UltimateCarouselSettingsModel, UltimateCarouselSettings>();
        }

        protected override void RegisterPluginServices(ContainerBuilder builder)
        {
            if (PluginHelper.IsPluginInstalled(Constants.PluginSystemName))
            {
                builder.RegisterType<CarouselService>().As<ICarouselService>().InstancePerLifetimeScope();
            }
        }

        protected override void RegisterPluginRepositories(ContainerBuilder builder)
        {
            RegisterPluginRepository<UCarousel>(builder);
            RegisterPluginRepository<CarouselItem>(builder);
        }
    }
}