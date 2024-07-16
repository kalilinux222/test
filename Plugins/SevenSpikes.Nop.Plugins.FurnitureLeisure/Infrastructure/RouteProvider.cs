using Nop.Web.Framework.Localization;
using SevenSpikes.Nop.Framework.ActionFilters;
using SevenSpikes.Nop.Framework.Routing;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.ActionFilters;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure
{
    public class RouteProvider : BaseRouteProvider
    {
        public RouteProvider() : base(pluginControllersNamespace: Plugin.DefaultAdminControllerNamespace, shouldRegisterPluginViewLocations: true)
        {
        }

        protected override string PluginSystemName => Plugin.SystemName;

        protected override void RegisterRoutesAccessibleByName(System.Web.Routing.RouteCollection routes)
        {
            routes.MapLocalizedRoute("RequestCatalog",
                         "requestcatalog",
                         new { controller = "FurnitureLeisure", action = "RequestCatalog" },
                         new[] { "SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers" });
        }

        protected override void RegisterPluginActionFilters()
        {
            var filterProvider = new GeneralActionFilterProvider();
            filterProvider.Add(new JCarouselAddSkuActionFilterFactory("JCarousel", "WidgetJCarousel"));

            FilterProviders.Providers.Add(filterProvider);
        }
    }
}