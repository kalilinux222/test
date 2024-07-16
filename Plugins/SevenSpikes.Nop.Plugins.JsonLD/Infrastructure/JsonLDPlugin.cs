using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Framework.Plugin;
using SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants;
using System.Web.Routing;

namespace SevenSpikes.Nop.Plugins.JsonLD.Infrastructure
{
    public class JsonLdPlugin : BaseAdminWidgetPlugin7Spikes
    {
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public JsonLdPlugin(WidgetSettings widgetSettings,
            ISettingService settingService) :
            base(Plugin.MenuItems, Plugin.ResourceName, Plugin.FolderName,
                null, false, Plugin.UrlInStore)
        {
            _widgetSettings = widgetSettings;
            _settingService = settingService;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Settings";
            controllerName = "JsonLdAdmin";
            routeValues = null;
        }

        public override void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "WidgetJsonLd";
            controllerName = "JsonLd";
            routeValues = new RouteValueDictionary { { "Namespaces", Plugin.PluginControllersNamespace }, { "area", null }, { "widgetZone", widgetZone } };
        }

        protected override void InstallAdditionalSettings()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Plugin.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
        }
    }
}
