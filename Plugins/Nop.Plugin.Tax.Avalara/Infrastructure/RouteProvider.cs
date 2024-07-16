using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Tax.Avalara.Infrastructure
{
    /// <summary>
    /// Represents route provider of Avalara tax provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Tax.Avalara.Tax.List",
                "Admin/Tax/List",
                new { controller = "OverriddenTax", action = "List" },
                new[] { "Nop.Plugin.Tax.Avalara.Controllers" });

            routes.MapRoute("Plugin.Tax.Avalara.Tax.MarkAsPrimaryProvider",
                "Admin/Tax/MarkAsPrimaryProvider",
                new { controller = "OverriddenTax", action = "MarkAsPrimaryProvider" },
                new[] { "Nop.Plugin.Tax.Avalara.Controllers" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1; //set a value that is greater than the default one in Nop.Web to override routes
    }
}