using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Framework.Plugin;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Data;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using System.Web.Routing;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure
{
    public class FurnitureLeisurePlugin : BaseAdminWidgetPlugin7Spikes
    {
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;

        public FurnitureLeisurePlugin(ISettingService settingService, WidgetSettings widgetSettings, FurnitureLeisureObjectContext dbContext)
            : base(Plugin.MenuItems, Plugin.ResourceName, Plugin.FolderName, dbContext, Plugin.IsTrialVersion, Plugin.UrlInStore)
        {
            _settingService = settingService;
            _widgetSettings = widgetSettings;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            controllerName = "FurnitureLeisureAdmin";
            actionName = "Settings";
            routeValues = null;
        }

        public override void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            controllerName = "FurnitureLeisure";
            actionName = "GetForWidgetZone";
            routeValues = new RouteValueDictionary { { "area", null }, { "widgetZone", widgetZone } };
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