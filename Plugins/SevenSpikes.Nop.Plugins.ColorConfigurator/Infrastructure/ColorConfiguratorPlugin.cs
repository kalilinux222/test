using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Tasks;
using SevenSpikes.Nop.Framework.Plugin;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Domain.Settings;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure
{
    public class ColorConfiguratorPlugin : BaseAdminPlugin7Spikes
    {
        private readonly IProductTemplateService _productTemplateService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ColorConfiguratorSettings _colorConfiguratorSettings;
        private readonly WidgetSettings _widgetSettings;

        public ColorConfiguratorPlugin(
            IProductTemplateService productTemplateService, 
            IScheduleTaskService scheduleTaskService, 
            ISettingService settingService, 
            ColorConfiguratorSettings colorConfiguratorSettings,
            WidgetSettings widgetSettings)
            : base(Plugin.MenuItems, Plugin.ResourceName, Plugin.FolderName, null, Plugin.IsTrialVersion, Plugin.UrlInStore)
        {
            _productTemplateService = productTemplateService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _colorConfiguratorSettings = colorConfiguratorSettings;
            _widgetSettings = widgetSettings;
        }

        protected override void InstallAdditionalSettings()
        {
            InstallScheduleTask();
            InstallProductTemplates();

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Plugin.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
        }

        protected override void UninstallAdditionalSettings()
        {
            ScheduleTask scheduleTask = _scheduleTaskService.GetTaskById(_colorConfiguratorSettings.ScheduledTaskId);

            if (scheduleTask != null)
            {
                _scheduleTaskService.DeleteTask(scheduleTask);
            }

            var productTemplates = _productTemplateService.GetAllProductTemplates().Where(x => x.ViewPath.EndsWith(Plugin.ColorConfiguratorPattern));

            foreach (var productTemplate in productTemplates)
            {
                _productTemplateService.DeleteProductTemplate(productTemplate);
            }
        }


        private void InstallScheduleTask()
        {
            var scheduleTask = new ScheduleTask()
            {
                Name = "Color Configurator - Delete Images Task",
                Seconds = _colorConfiguratorSettings.ScheduledTaskIntervalInSeconds,
                Type = Plugin.ScheduledTaskType,
                Enabled = true,
                StopOnError = false
            };

            _scheduleTaskService.InsertTask(scheduleTask);

            _colorConfiguratorSettings.ScheduledTaskId = scheduleTask.Id;

            _settingService.SaveSetting(_colorConfiguratorSettings);
        }

        private void InstallProductTemplates()
        {
            var nonConditionalProductTemplate = new ProductTemplate
            {
                Name = "Non-Conditional Color configurator",
                ViewPath = Plugin.NonConditionalProductTemplate,
                DisplayOrder = 150
            };

            _productTemplateService.InsertProductTemplate(nonConditionalProductTemplate);

            var conditionalProductTemplate = new ProductTemplate
            {
                Name = "Conditional Color configurator",
                ViewPath = Plugin.ConditionalProductTemplate,
                DisplayOrder = 151
            };

            _productTemplateService.InsertProductTemplate(conditionalProductTemplate);
        }
    }
}