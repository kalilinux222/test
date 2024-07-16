using SevenSpikes.Nop.Framework.Plugin;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants
{
    public class Plugin
    {
        public const string Name = "Nop Color Configuratopr";
        public const string ResourceName = "SevenSpikes.ColorConfigurator";
        public const string SystemName = "SevenSpikes.Nop.Plugins.ColorConfigurator";
        public const string FolderName = "SevenSpikes.Nop.Plugins.ColorConfigurator";
        public const string UrlInStore = "http://www.nop-templates.com/ColorConfigurator-plugin-for-nopcommerce";

        public const string ColorConfiguratorPattern = "ColorConfigurator";
        public const string NonConditionalProductTemplate = "ProductTemplate.NonConditional" + ColorConfiguratorPattern;
        public const string ConditionalProductTemplate = "ProductTemplate.Conditional" + ColorConfiguratorPattern;

        public const string ScheduledTaskType = "SevenSpikes.Nop.Plugins.ColorConfigurator.Tasks.ColorConfiguratorTask, SevenSpikes.Nop.Plugins.ColorConfigurator";

        public const string DefaultAdminControllerNamespace = "SevenSpikes.Nop.Plugins.ColorConfigurator.Controllers.Admin";

        public const string ImagesPath = "/Content/files/ColorConfiguration/";
        public const string ImagesVirtualPath = "~/Content/files/ColorConfiguration";

        public static bool IsTrialVersion
        {
            get
            {
#if TRIAL
                    return true;
#endif

                return false;
            }
        }

        public static List<MenuItem7Spikes> MenuItems => new List<MenuItem7Spikes>
        {
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.ColorConfigurator.Admin.Submenus.Settings",
                SubMenuRelativePath = "ColorConfiguratorAdmin/Settings"
            }
        };
    }
}