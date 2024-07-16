using SevenSpikes.Nop.Framework.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants
{
    public static class Plugin
    {
        public const string Name = "Nop Json LD";
        public const string ResourceName = "SevenSpikes.JsonLd";
        public const string SystemName = "SevenSpikes.Nop.Plugins.JsonLd";
        public const string FolderName = "SevenSpikes.Nop.Plugins.JsonLd";
        public const string UrlInStore = "http://www.nop-templates.com";

        public const string PluginControllersNamespace = "SevenSpikes.Nop.Plugins.JsonLd.Controllers";
        public const string DefaultAdminControllerNamespace = "SevenSpikes.Nop.Plugins.JsonLd.Controllers.Admin";

        public static List<MenuItem7Spikes> MenuItems => new List<MenuItem7Spikes>
        {
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.JsonLd.Admin.Submenus.Settings",
                SubMenuRelativePath = "JsonLdAdmin/Settings"
            }
        };

        /* Cache Key Details: 
         * {0} - categoryId
         * {1} - customerId
         * {2} - workingLanguageId
         * {3} - storeId
         */
        public const string JSONLD_CATEGORY_CACHE_KEY = "jsonld-category-cache-{0}-{1}-{2}-{3}";

        /* Cache Key Details: 
         * {0} - categoryId
         * {1} - customerId
         * {2} - workingLanguageId
         * {3} - storeId
         */
        public const string JSONLD_PRODUCT_CACHE_KEY = "jsonld-product-cache-{0}-{1}-{2}-{3}";
    }
}
