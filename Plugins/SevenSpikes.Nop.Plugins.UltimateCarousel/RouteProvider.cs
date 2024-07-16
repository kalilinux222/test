using System.Web.Mvc;
using System.Web.Routing;
using SevenSpikes.Nop.Framework.Routing;
using Nop.Web.Framework.Localization;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel
{
    class RouteProvider : BaseRouteProvider
    {
        public RouteProvider():base(false, Constants.PluginControllerName, true)
        {}

        /*
        protected override void RegisterRoutesAccessibleByName(RouteCollection routes)
        {
            routes.MapLocalizedRoute(Constants.AllCarouselsRouteName,
                                     Constants.AllCarouselsRouteName,
                                     new { controller = "UltimateCarousel", action = "AllCarousels" },
                                     new[] { "SevenSpikes.Nop.Plugins.UltimateCarousel.Controllers" });
        }
         */

        protected override string PluginSystemName
        {
            get { return Constants.PluginSystemName; }
        }
    }
}