using SevenSpikes.Nop.Framework.ActionFilters;
using SevenSpikes.Nop.Framework.Routing;
using SevenSpikes.Nop.Plugins.JsonLD.ActionFilters;
using SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.Infrastructure
{
    public class RouteProvider : BaseRouteProvider
    {
        public RouteProvider() : base (pluginControllersNamespace: Plugin.DefaultAdminControllerNamespace, shouldRegisterPluginViewLocations: true)
        {

        }

        protected override void RegisterPluginActionFilters()
        {
            var filterProvider = new GeneralActionFilterProvider();

            filterProvider.Add(new JsonLdCategoryFilterFactory());
            filterProvider.Add(new JsonLdProductFilterFactory());

            FilterProviders.Providers.Add(filterProvider);
        }

        protected override string PluginSystemName => Plugin.SystemName;
    }
}
