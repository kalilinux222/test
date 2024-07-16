using SevenSpikes.Nop.Framework.ActionFilters;
using SevenSpikes.Nop.Framework.Routing;
using SevenSpikes.Nop.Plugins.ColorConfigurator.ActionFilters;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure
{
    public class RouteProvider : BaseRouteProvider
    {
        public RouteProvider() : base(pluginControllersNamespace: Plugin.DefaultAdminControllerNamespace, shouldRegisterPluginViewLocations: true)
        {
        }

        protected override void RegisterPluginActionFilters()
        {
            var filterProvider = new GeneralActionFilterProvider();

            filterProvider.Add(new ShoppingCartActionFilterFactory("ShoppingCart", "Cart"));
            filterProvider.Add(new MiniShoppingCartActionFilterFactory("ShoppingCart", "FlyoutShoppingCart"));
            filterProvider.Add(new MiniShoppingCartActionFilterFactory("NopAjaxCartShoppingCart", "NopAjaxCartFlyoutShoppingCart"));

            FilterProviders.Providers.Add(filterProvider);
        }

        protected override string PluginSystemName => Plugin.SystemName;
    }
}