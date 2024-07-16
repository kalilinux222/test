using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Conditions.Services;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.Plugin;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Plugin
{
    public class UltimateCarouselPlugin : BaseAdminPlugin7Spikes, IWidgetPlugin
    {
        private readonly IInstallHelper _installHelper;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly IConditionsInstallerService _conditionsInstallerService;

        private static readonly List<MenuItem7Spikes> MenuItems = new List<MenuItem7Spikes>
        {
            new MenuItem7Spikes
            { 
                SubMenuName = "SevenSpikes.UltimateCarousel.Admin.Submenus.Settings", 
                SubMenuRelativePath = "UltimateCarouselAdmin/Settings"
            },
            new MenuItem7Spikes
            { 
                SubMenuName = "SevenSpikes.UltimateCarousel.Admin.Submenus.ManageCarousels", 
                SubMenuRelativePath = "UltimateCarouselAdmin/List"
            }
        };

        private static bool IsTrialVersion
        {
            get
            {
#if TRIAL
                return true;
#endif
                return false;
            }
        }

        public UltimateCarouselPlugin(IInstallHelper installHelper, 
            UltimateCarouselObjectContext dbContext, 
            WidgetSettings widgetSettings, 
            ISettingService settingService, 
            IConditionsInstallerService conditionsInstallerService)
            : base(MenuItems, Constants.PluginResourceName, Constants.PluginFolderName, dbContext, IsTrialVersion, Constants.PluginUrlInStore)
        {
            _installHelper = installHelper;
            _widgetSettings = widgetSettings;
            _settingService = settingService;
            _conditionsInstallerService = conditionsInstallerService;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            controllerName = "UltimateCarouselAdmin";
            actionName = "Settings";
            routeValues = null;
        }

        public IList<string> GetWidgetZones()
        {
            return _installHelper.GetSupportedWidgetZones(PluginFolderName).ToList();
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "UltimateCarousel";
            controllerName = "UltimateCarousel";
            routeValues = new RouteValueDictionary { { "Namespaces", Constants.PluginControllerName }, { "area", null }, { "widgetZone", widgetZone } };
        }

        protected override void InstallAdditionalSettings()
        {
            _conditionsInstallerService.InstallConditionsForEntity(Constants.EntityType);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Constants.PluginSystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Constants.PluginSystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
        }

        protected override void UninstallAdditionalSettings()
        {
            _conditionsInstallerService.UnInstallConditionsForEntity(Constants.EntityType);
        }
    }
}
